﻿using Ardalis.GuardClauses;
using CoreLib.Utils;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CoreLib.EntityFramework.Features.Encryption;

public static class DatabaseFacadeExtensions
{
    /// <summary>
    /// Do migration with additional encryption of non-ecrypted properties.
    /// Use it if there are already some data in DB that must be encrypted.
    /// !!! ONLY PostgreSQL IS SUPPORTED. !!!
    /// </summary>
    public static Task MigrateWithEncryptingMigratorAsync(this DatabaseFacade databaseFacade, CancellationToken cancellationToken = default)
    {
        Defend.Against.Null(databaseFacade);
        return EncryptingMigrator.MigrateWithEncriptingMigratorAsync(databaseFacade, cancellationToken);
    }
}
