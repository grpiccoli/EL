using ELO.Data;
using ELO.Models;
using ELO.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Pluralize.NET.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ELO.Services
{
    public class SeedService : ISeed
    {
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        public IConfiguration Configuration { get; }
        private readonly IHostingEnvironment _environment;
        private readonly string _os;
        private readonly string _conn;
        private readonly ApplicationDbContext _context;
        private readonly ILookupNormalizer _normalizer;
        public SeedService(
            ILogger<SeedService> logger,
            IStringLocalizer<SeedService> localizer,
            IConfiguration configuration,
            IHostingEnvironment environment,
            ApplicationDbContext context,
            ILookupNormalizer normalizer
            //IUser user
            //Bulk bulk
            )
        {
            _logger = logger;
            _localizer = localizer;
            Configuration = configuration;
            _environment = environment;
            _os = Environment.OSVersion.Platform.ToString();
            _conn = Configuration.GetConnectionString($"{_os}Connection");
            _context = context;
            _normalizer = normalizer;
            //_user = user;
            //_bulk = bulk;
        }
        public async Task Seed()
        {
            try
            {
                await AddProcedure().ConfigureAwait(false);
                await Users().ConfigureAwait(false);
                var tsvPath = Path.Combine(_environment.ContentRootPath, "Data");

                if (!_context.Continents.Any())
                    await Insert<Continent>(tsvPath).ConfigureAwait(false);
                if (!_context.Countries.Any())
                    await Insert<Country>(tsvPath).ConfigureAwait(false);

                if (!_context.Regions.Any())
                    await Insert<Region>(tsvPath).ConfigureAwait(false);
                if (!_context.Provinces.Any())
                    await Insert<Province>(tsvPath).ConfigureAwait(false);
                if (!_context.Communes.Any())
                    await Insert<Commune>(tsvPath).ConfigureAwait(false);

                if (!_context.Arrivals.Any())
                    await Insert<Arrival>(tsvPath).ConfigureAwait(false);

                if (!_context.Exports.Any())
                    await Insert<Export>(tsvPath).ConfigureAwait(false);

                if (!_context.Stations.Any())
                    await Insert<Station>(tsvPath).ConfigureAwait(false);

                if (!_context.Coordinates.Any())
                    await Insert<Coordinate>(tsvPath).ConfigureAwait(false);

                if (!_context.Companies.Any())
                    await Insert<Company>(tsvPath).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["There has been an error while seeding the database."]);
                throw;
            }
        }
        public async Task AddProcedure()
        {
            string query = "select * from sysobjects where type='P' and name='BulkInsert'";
            var sp = @"CREATE PROCEDURE BulkInsert(@TableName NVARCHAR(50), @Tsv NVARCHAR(100))
AS
BEGIN 
DECLARE @SQLSelectQuery NVARCHAR(MAX)=''
SET @SQLSelectQuery = 'BULK INSERT ' + @TableName + ' FROM ' + QUOTENAME(@Tsv) + ' WITH (DATAFILETYPE=''widechar'')'
  exec(@SQLSelectQuery)
END";
            bool spExists = false;
            using (SqlConnection connection = new SqlConnection(_conn))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = query;
                    connection.Open();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            spExists = true;
                            break;
                        }
                    }
                    if (!spExists)
                    {
                        command.CommandText = sp;
                        using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                spExists = true;
                                break;
                            }
                        }
                    }
                    connection.Close();
                }
            }
        }
        public async Task Insert<TSource>(string path)
        {
            var name = new Pluralizer().Pluralize(typeof(TSource).ToString().Split(".").Last());
            _context.Database.SetCommandTimeout(10000);
            var tableName = $"dbo.{name}";
            var tsv = Path.Combine(path, $"{name}.tsv");
            var tmp = Path.Combine(Path.GetTempPath(), $"{name}.tsv");
            File.Copy(tsv, tmp, true);
            await _context.Database
                .ExecuteSqlCommandAsync($"BulkInsert @p0, @p1;", tableName, tmp)
                .ConfigureAwait(false);
            File.Delete(tmp);
            return;
        }
        public async Task Users()
        {
            var roleStore = new RoleStore<AppRole>(_context);
            var userStore = new UserStore<AppUser>(_context);

            if (!_context.AppUserRole.Any())
            {
                if (!_context.Users.Any())
                {
                    if (!_context.AppRole.Any())
                    {
                        var applicationRoles = new List<AppRole> { };
                        foreach (var item in RoleData.AppRoles)
                        {
                            applicationRoles.Add(
                                new AppRole
                                {
                                    CreatedDate = DateTime.Now,
                                    Name = item,
                                    Description = "",
                                    NormalizedName = item.ToLower()
                                });
                        };

                        foreach (var role in applicationRoles)
                        {
                            await _context.AppRole.AddAsync(role)
                                .ConfigureAwait(false);
                        }
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                    }

                    var users = new UserInitializerVM[]
                    {
                        new UserInitializerVM
                        {
                            Name = "WebMaster",
                            Email = "adminmit@bibliomit.cl",
                            Roles = RoleData.AppRoles.ToArray(),
                            Key = "34#$erERdfDFcvCV",
                            Image = "/images/logo.svg",
                            Rating = 10,
                            Claims = ClaimData.UserClaims.ToArray()
                        },
                    };

                    foreach (var item in users)
                    {
                        var user = new AppUser
                        {
                            UserName = item.Name,
                            NormalizedUserName = _normalizer.Normalize(item.Name),
                            Email = item.Email,
                            NormalizedEmail = _normalizer.Normalize(item.Email),
                            EmailConfirmed = true,
                            LockoutEnabled = false,
                            SecurityStamp = Guid.NewGuid().ToString(),
                            ProfileImageUrl = item.Image
                        };

                        var hasher = new PasswordHasher<AppUser>();
                        var hashedPassword = hasher.HashPassword(user, item.Key);
                        user.PasswordHash = hashedPassword;

                        foreach (var claim in item.Claims)
                        {
                            user.Claims.Add(new IdentityUserClaim<string>
                            {
                                ClaimType = claim,
                                ClaimValue = claim
                            });
                        }
                        foreach (var role in item.Roles)
                        {
                            var roller = _context.Roles.SingleOrDefault(r => r.Name == role);
                            user.Roles.Add(new IdentityUserRole<string>
                            {
                                UserId = user.Id,
                                RoleId = roller.Id
                            });
                        }
                        await _context.Users.AddAsync(user)
                            .ConfigureAwait(false);
                    }
                    await _context.SaveChangesAsync()
                        .ConfigureAwait(false);
                }
            }
            return;
        }
    }
}
