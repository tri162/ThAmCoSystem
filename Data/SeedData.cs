using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ThreeAmigosWebApp.Models;
using System.Linq;

public class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        CreateRoles(roleManager);

        SeedProducts(context);

        CreateDefaultAdminUser(userManager, roleManager);
    }

    private static void CreateRoles(RoleManager<IdentityRole> roleManager)
    {
        var roleNames = new[] { "Admin", "Staff", "Customer" };

        foreach (var roleName in roleNames)
        {
            var roleExist = roleManager.RoleExistsAsync(roleName).Result;
            if (!roleExist)
            {
                var role = new IdentityRole(roleName);
                roleManager.CreateAsync(role).Wait();
            }
        }
    }

    private static void SeedProducts(ApplicationDbContext context)
    {
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product
                {
                    Name = "Laptop",
                    Price = 1000,
                    StockQuantity = 50,
                    Description = "A high-performance laptop for gaming and work.",
                    ImageFileName = "laptop.jpg"
                },
                new Product
                {
                    Name = "Smartphone",
                    Price = 500,
                    StockQuantity = 100,
                    Description = "A feature-packed smartphone with excellent camera quality.",
                    ImageFileName = "phone.jpg"
                },
                new Product
                {
                    Name = "Headphones",
                    Price = 150,
                    StockQuantity = 200,
                    Description = "Noise-canceling headphones with excellent sound quality.",
                    ImageFileName = "headphones.jpg"
                },
                new Product
                {
                    Name = "Shoes",
                    Price = 150,
                    StockQuantity = 200,
                    Description = "Noise-canceling headphones with excellent sound quality.",
                    ImageFileName = "shoes.jpg"
                },
                new Product
                {
                    Name = "Bag",
                    Price = 150,
                    StockQuantity = 200,
                    Description = "Noise-canceling headphones with excellent sound quality.",
                    ImageFileName = "bag.jpg"
                }
            );

            context.SaveChanges();
        }
    }

    private static void CreateDefaultAdminUser(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        if (!userManager.Users.Any())
        {
            var adminUser = new User
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                Address = "123 Main St, City, Country",
                PhoneNumber = "1234567890",
                Funds = 100000 
            };

            var result = userManager.CreateAsync(adminUser, "Admin@123").Result;
            if (result.Succeeded)
            {
                if (!roleManager.RoleExistsAsync("Admin").Result)
                {
                    var role = new IdentityRole("Admin");
                    roleManager.CreateAsync(role).Wait();
                }

                userManager.AddToRoleAsync(adminUser, "Admin").Wait();
            }
        }
    }
}
