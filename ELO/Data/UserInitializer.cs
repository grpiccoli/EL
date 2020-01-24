using ELO.Data;
using ELO.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ELO.Models
{
    public class UserInitializer
    {
        public static Task Initialize(ApplicationDbContext context)
        {
            var roleStore = new RoleStore<AppRole>(context);
            var userStore = new UserStore<AppUser>(context);

            if (!context.AppUserRole.Any())
            {
                if (!context.Users.Any())
                {
                    if (!context.AppRole.Any())
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
                            context.AppRole.Add(role);
                        }
                        context.SaveChanges();
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
                            NormalizedUserName = item.Name.ToLower(),
                            Email = item.Email,
                            NormalizedEmail = item.Email.ToLower(),
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
                        context.Users.Add(user);
                        context.SaveChanges();

                        foreach (var role in item.Roles)
                        {
                            var roller = context.Roles.SingleOrDefault(r => r.Name == role);
                            user.Roles.Add(new IdentityUserRole<string> {
                                UserId = user.Id,
                                RoleId = roller.Id
                            });
                        }
                        context.Update(user);
                        context.SaveChanges();

                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
