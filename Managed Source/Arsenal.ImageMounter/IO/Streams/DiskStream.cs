﻿//  DiskStream.vb
//  Stream implementation for direct access to raw disk data.
//  
//  Copyright (c) 2012-2023, Arsenal Consulting, Inc. (d/b/a Arsenal Recon) <http://www.ArsenalRecon.com>
//  This source code and API are available under the terms of the Affero General Public
//  License v3.
// 
//  Please see LICENSE.txt for full license terms, including the availability of
//  proprietary exceptions.
//  Questions, comments, or requests for clarification: http://ArsenalRecon.com/contact/
// 

using Arsenal.ImageMounter.IO.Native;
using DiscUtils.Streams.Compatibility;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Arsenal.ImageMounter.IO.Streams;

/// <summary>
/// A AligningStream derived class that represents disk devices. It aligns I/O requests to complete
/// sectors for compatibility with underlying device.
/// </summary>
public class DiskStream : AligningStream
{

    /// <summary>
    /// Initializes an DiskStream object for an open disk device.
    /// </summary>
    /// <param name="SafeFileHandle">Open file handle for disk device.</param>
    /// <param name="AccessMode">Access to request for stream.</param>
    protected internal DiskStream(SafeFileHandle SafeFileHandle, FileAccess AccessMode)
        : base(new FileStream(SafeFileHandle, AccessMode, bufferSize: 1),
               Alignment: (NativeStruct.GetDiskGeometry(SafeFileHandle)?.BytesPerSector) ?? 512,
               ownsBaseStream: true)
    {
    }

    private long? cachedLength;

    /// <summary>
    /// Initializes an DiskStream object for an open disk device.
    /// </summary>
    /// <param name="SafeFileHandle">Open file handle for disk device.</param>
    /// <param name="AccessMode">Access to request for stream.</param>
    /// <param name="DiskSize">Size that should be returned by Length property</param>
    protected internal DiskStream(SafeFileHandle SafeFileHandle, FileAccess AccessMode, long DiskSize)
        : base(new FileStream(SafeFileHandle, AccessMode, bufferSize: 1),
               Alignment: (NativeStruct.GetDiskGeometry(SafeFileHandle)?.BytesPerSector) ?? 512,
               ownsBaseStream: true)
    {
        cachedLength = DiskSize;
    }

    public SafeFileHandle SafeFileHandle => ((FileStream)BaseStream).SafeFileHandle;

    /// <summary>
    /// Retrieves raw disk size.
    /// </summary>
    public override long Length
    {
        get
        {
            cachedLength ??= NativeStruct.GetDiskSize(SafeFileHandle);

            return cachedLength ?? throw new NotSupportedException("Disk size not available");
        }
    }

    private bool size_from_vbr;

    public bool SizeFromVBR
    {
        get => size_from_vbr;
        set
        {
            if (value)
            {
                cachedLength = GetVBRPartitionLength();
                if (!cachedLength.HasValue)
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                cachedLength = NativeStruct.GetDiskSize(SafeFileHandle);
                if (!cachedLength.HasValue)
                {
                    throw new NotSupportedException();
                }
            }

            size_from_vbr = value;
        }
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    public override void SetLength(long value) => throw new NotImplementedException();

    /// <summary>
    /// Get partition length as indicated by VBR. Valid for volumes with formatted file system.
    /// </summary>
    public long? GetVBRPartitionLength()
    {

        var bytesPerSector = NativeStruct.GetDiskGeometry(SafeFileHandle)?.BytesPerSector ?? 512;

        var vbr = bytesPerSector <= 512
            ? stackalloc byte[bytesPerSector]
            : new byte[bytesPerSector];

        Position = 0;

        if (this.Read(vbr) < bytesPerSector)
        {
            return default;
        }

        var vbr_sector_size = MemoryMarshal.Read<short>(vbr.Slice(0xB));

        if (vbr_sector_size <= 0)
        {
            return default;
        }

        long total_sectors;

        total_sectors = MemoryMarshal.Read<ushort>(vbr.Slice(0x13));

        if (total_sectors == 0)
        {

            total_sectors = MemoryMarshal.Read<uint>(vbr.Slice(0x20));

        }

        if (total_sectors == 0)
        {

            total_sectors = MemoryMarshal.Read<long>(vbr.Slice(0x28));

        }

        return total_sectors < 0 ? default : (long?)(total_sectors * vbr_sector_size);
    }
}
