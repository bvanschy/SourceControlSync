﻿using SourceControlSync.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public interface ISourceRepository
    {
        string ConnectionString { set; }
        Task DownloadChangesAsync(Push push, string root, CancellationToken token);
    }
}
