﻿//  
//  Copyright (c) 2012-2023, Arsenal Consulting, Inc. (d/b/a Arsenal Recon) <http://www.ArsenalRecon.com>
//  This source code and API are available under the terms of the Affero General Public
//  License v3.
// 
//  Please see LICENSE.txt for full license terms, including the availability of
//  proprietary exceptions.
//  Questions, comments, or requests for clarification: http://ArsenalRecon.com/contact/
// 

using System;
using System.Threading;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Arsenal.ImageMounter.IO.ConsoleIO;

public class ConsoleSpinProgress : IDisposable
{

    public Timer Timer { get; }

    private char CurrentChar;

    public ConsoleSpinProgress(int dueTime, int period)
    {
        Timer = new Timer(Tick);
        Timer.Change(dueTime, period);
    }

    public ConsoleSpinProgress(TimeSpan dueTime, TimeSpan period)
    {
        Timer = new Timer(Tick);
        Timer.Change(dueTime, period);
    }

    private void Tick(object? o) => UpdateConsoleSpinProgress(ref CurrentChar);

    public static void UpdateConsoleSpinProgress(ref char chr)
    {
        switch (chr)
        {
            case '\\':
                {
                    chr = '|';
                    break;
                }
            case '|':
                {
                    chr = '/';
                    break;
                }
            case '/':
                {
                    chr = '-';
                    break;
                }

            default:
                {
                    chr = '\\';
                    break;
                }
        }

        lock (ConsoleSupport.ConsoleSync)
        {
            Console.ForegroundColor = ConsoleProgressBar.ConsoleProgressBarColor;

            Console.Write(chr);
            Console.Write('\b');

            Console.ResetColor();
        }
    }

    private bool disposedValue; // To detect redundant calls

    // IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                Timer.Dispose();
                Console.WriteLine(" ");

            }

            // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.

            // TODO: set large fields to null.
        }

        disposedValue = true;
    }

    // TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    ~ConsoleSpinProgress()
    {
        // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(false);
    }

    // This code added by Visual Basic to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(true);
        // TODO: uncomment the following line if Finalize() is overridden above.
        GC.SuppressFinalize(this);
    }
}