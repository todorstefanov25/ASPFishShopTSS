using FishShopASP.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace FishShopASP.Services
{
    public static class ApplicationBuilderExtension
    {
        public static async Task<IApplicationBuilder> PrepareDataBase(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var services = scope.ServiceProvider;

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<Client>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                //Sazdavane na roles
                await SeedRolesAsync(roleManager);
                //sazdavane na SUPER ADMIN s vsi4kite mu roli
                await SeedSuperAdminAsync(userManager);
                await SeedShopDataAsync(context, userManager);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred seeding the DB.");
            }

            return app;
        }
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            //foreach (var role in Enum.GetValues(Roles))
            //{
            //                    var roleExist = await roleManager.RoleExistsAsync(role); 
            //    if (!roleExist)
            //    { }
            //}
           
                //Seed Roles
                foreach (var roleName in new[] { "Admin", "Client", "Guest" })
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
            }

            public static async Task SeedSuperAdminAsync(UserManager<Client> userManager)
            {
                //Seed Default User
                var defaultUser = new Client
                {
                    UserName = "superadmin",
                    Email = "superadmin@gmail.com",
                    FirstName = "Tonya",
                    LastName = "Belezireva",
                    PhoneNumber = "0899999999",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
           
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    var result = await userManager.CreateAsync(defaultUser, "123!@#Qwe");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(defaultUser, "Admin");
                        //await userManager.AddToRoleAsync(defaultUser, Roles.Guest.ToString());
                        //await userManager.AddToRoleAsync(defaultUser, Roles.User.ToString());                    
                    }
                }
            }

            public static async Task SeedShopDataAsync(ApplicationDbContext context, UserManager<Client> userManager)
            {
                var categoryNames = new[]
                {
                    "Въдици",
                    "Макари",
                    "Влакна",
                    "Примамки",
                    "Куки и монтажи",
                    "Аксесоари"
                };

                foreach (var categoryName in categoryNames)
                {
                    if (!await context.Categories.AnyAsync(c => c.Name == categoryName))
                    {
                        context.Categories.Add(new Category { Name = categoryName });
                    }
                }

                await context.SaveChangesAsync();

                var oldSeedCategoryNames = new[] { "Аквариумни риби", "Хищни риби", "Езерни риби", "Храна", "Аквариуми" };
                var emptyOldCategories = await context.Categories
                    .Where(c => oldSeedCategoryNames.Contains(c.Name) && !c.Products.Any())
                    .ToListAsync();

                if (emptyOldCategories.Any())
                {
                    context.Categories.RemoveRange(emptyOldCategories);
                    await context.SaveChangesAsync();
                }

                var categories = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);
                var now = DateTime.Now;
                const string imageBaseUrl = "https://raw.githubusercontent.com/todorstefanov25/ASPFishShopTSS/main/Images/products";
                var products = new[]
                {
                    new Product { CatalogNumber = 1001, Name = "Спининг въдица 2.40 м", Price = 59.90, CategoryId = categories["Въдици"], Description = "Лека карбонова въдица за риболов с изкуствени примамки.", imageURL = $"{imageBaseUrl}/Spining.jpg", RegOn = now.AddDays(-18) },
                    new Product { CatalogNumber = 1002, Name = "Телескопична въдица 3.60 м", Price = 42.50, CategoryId = categories["Въдици"], Description = "Компактна телескопична въдица за плувка и лек дънен риболов.", imageURL = $"{imageBaseUrl}/Teleskop.jpg", RegOn = now.AddDays(-16) },
                    new Product { CatalogNumber = 1003, Name = "Фидер въдица 3.90 м", Price = 89.00, CategoryId = categories["Въдици"], Description = "Чувствителна фидер въдица с резервни върхове за речен и язовирен риболов.", imageURL = $"{imageBaseUrl}/Fider.jpg", RegOn = now.AddDays(-14) },
                    new Product { CatalogNumber = 1004, Name = "Шаранска въдица 3.60 м", Price = 115.00, CategoryId = categories["Въдици"], Description = "Здрава двуколенна въдица за шарански риболов на дистанция.", imageURL = $"{imageBaseUrl}/Sharan.JPEG", RegOn = now.AddDays(-13) },
                    new Product { CatalogNumber = 1005, Name = "Болонезе въдица 5 м", Price = 74.90, CategoryId = categories["Въдици"], Description = "Удобна болонезе въдица за контролирано водене на линията в течение.", imageURL = $"{imageBaseUrl}/Boloneze.jpg", RegOn = now.AddDays(-11) },
                    new Product { CatalogNumber = 2001, Name = "Макара 3000 с преден аванс", Price = 69.90, CategoryId = categories["Макари"], Description = "Универсална макара за спининг и плувка с плавен преден аванс.", imageURL = $"{imageBaseUrl}/MakaraSpredenAvans.jpg", RegOn = now.AddDays(-10) },
                    new Product { CatalogNumber = 2002, Name = "Шаранска макара baitrunner", Price = 129.50, CategoryId = categories["Макари"], Description = "Макара с байтрънър система и голяма шпула за дълги замятания.", imageURL = $"{imageBaseUrl}/makaraBraituner.jpg", RegOn = now.AddDays(-9) },
                    new Product { CatalogNumber = 3001, Name = "Монофилно влакно 0.25 мм", Price = 9.90, CategoryId = categories["Влакна"], Description = "Универсално монофилно влакно с добра здравина на възел.", imageURL = $"{imageBaseUrl}/monofil.jpg", RegOn = now.AddDays(-8) },
                    new Product { CatalogNumber = 3002, Name = "Плетено влакно 0.12 мм", Price = 24.90, CategoryId = categories["Влакна"], Description = "Четиринишково плетено влакно за спининг с висока чувствителност.", imageURL = $"{imageBaseUrl}/monofil.jpg", RegOn = now.AddDays(-7) },
                    new Product { CatalogNumber = 4001, Name = "Силиконови примамки комплект", Price = 14.90, CategoryId = categories["Примамки"], Description = "Комплект силикони в различни цветове за костур, бяла риба и щука.", imageURL = $"{imageBaseUrl}/Silikoni.jpg", RegOn = now.AddDays(-6) },
                    new Product { CatalogNumber = 4002, Name = "Воблер minnow 9 см", Price = 11.40, CategoryId = categories["Примамки"], Description = "Плуващ воблер с активна игра за плитки участъци.", imageURL = $"{imageBaseUrl}/Vobler.jpg", RegOn = now.AddDays(-5) },
                    new Product { CatalogNumber = 5001, Name = "Куки номер 8 - 10 бр.", Price = 3.90, CategoryId = categories["Куки и монтажи"], Description = "Остри куки за плувка и фидер, подходящи за бяла риба и каракуда.", imageURL = $"{imageBaseUrl}/Kuki.jpg", RegOn = now.AddDays(-4) },
                    new Product { CatalogNumber = 5002, Name = "Готов фидер монтаж", Price = 5.50, CategoryId = categories["Куки и монтажи"], Description = "Практичен готов монтаж с хранилка, повод и вирбел.", imageURL = $"{imageBaseUrl}/FiderMontaj.jpg", RegOn = now.AddDays(-3) },
                    new Product { CatalogNumber = 6001, Name = "Кутия за такъми", Price = 29.90, CategoryId = categories["Аксесоари"], Description = "Органайзер с прегради за примамки, куки, вирбели и дребни аксесоари.", imageURL = $"{imageBaseUrl}/KutiqZaTakumi.jpg", RegOn = now.AddDays(-2) },
                    new Product { CatalogNumber = 6002, Name = "Кеп сгъваем", Price = 34.50, CategoryId = categories["Аксесоари"], Description = "Лек сгъваем кеп с телескопична дръжка за безопасно вадене на улова.", imageURL = $"{imageBaseUrl}/Kep.jpg", RegOn = now.AddDays(-1) }
                };

                foreach (var product in products)
                {
                    var existingProduct = await context.Products.FirstOrDefaultAsync(p => p.CatalogNumber == product.CatalogNumber);

                    if (existingProduct == null)
                    {
                        context.Products.Add(product);
                    }
                    else
                    {
                        existingProduct.Name = product.Name;
                        existingProduct.Price = product.Price;
                        existingProduct.CategoryId = product.CategoryId;
                        existingProduct.Description = product.Description;

                        if (string.IsNullOrWhiteSpace(existingProduct.imageURL)
                            || existingProduct.imageURL.StartsWith("/images/products/")
                            || existingProduct.imageURL.StartsWith("/uploads/products/"))
                        {
                            existingProduct.imageURL = product.imageURL;
                        }
                    }
                }

                await context.SaveChangesAsync();

                emptyOldCategories = await context.Categories
                    .Where(c => oldSeedCategoryNames.Contains(c.Name) && !c.Products.Any())
                    .ToListAsync();

                if (emptyOldCategories.Any())
                {
                    context.Categories.RemoveRange(emptyOldCategories);
                    await context.SaveChangesAsync();
                }

                var seedClients = new[]
                {
                    new Client { UserName = "ivan.petrov", Email = "ivan.petrov@example.com", FirstName = "Иван", LastName = "Петров", PhoneNumber = "0888123456", EmailConfirmed = true, PhoneNumberConfirmed = true },
                    new Client { UserName = "maria.georgieva", Email = "maria.georgieva@example.com", FirstName = "Мария", LastName = "Георгиева", PhoneNumber = "0888234567", EmailConfirmed = true, PhoneNumberConfirmed = true },
                    new Client { UserName = "petar.dimitrov", Email = "petar.dimitrov@example.com", FirstName = "Петър", LastName = "Димитров", PhoneNumber = "0888345678", EmailConfirmed = true, PhoneNumberConfirmed = true }
                };

                foreach (var client in seedClients)
                {
                    if (await userManager.FindByEmailAsync(client.Email!) == null)
                    {
                        var result = await userManager.CreateAsync(client, "123!@#Qwe");

                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(client, "Client");
                        }
                    }
                }

                var ivan = await userManager.FindByEmailAsync("ivan.petrov@example.com");
                var maria = await userManager.FindByEmailAsync("maria.georgieva@example.com");
                var petar = await userManager.FindByEmailAsync("petar.dimitrov@example.com");

                var sampleProducts = await context.Products
                    .Where(p => p.CatalogNumber == 1001 || p.CatalogNumber == 1003 || p.CatalogNumber == 4001 || p.CatalogNumber == 5001 || p.CatalogNumber == 6001)
                    .ToDictionaryAsync(p => p.CatalogNumber, p => p.Id);

                if (ivan != null && maria != null && petar != null && sampleProducts.Count == 5)
                {
                    var orderItems = new[]
                    {
                        new OrderItem { ClientId = ivan.Id, ProductId = sampleProducts[1001], Quantity = 2, RegOn = now.AddHours(-7) },
                        new OrderItem { ClientId = ivan.Id, ProductId = sampleProducts[4001], Quantity = 1, RegOn = now.AddHours(-6) },
                        new OrderItem { ClientId = maria.Id, ProductId = sampleProducts[1003], Quantity = 8, RegOn = now.AddHours(-5) },
                        new OrderItem { ClientId = maria.Id, ProductId = sampleProducts[6001], Quantity = 1, RegOn = now.AddHours(-4) },
                        new OrderItem { ClientId = petar.Id, ProductId = sampleProducts[5001], Quantity = 1, RegOn = now.AddHours(-3) }
                    };

                    foreach (var item in orderItems)
                    {
                        if (!await context.OrderItems.AnyAsync(o => o.ClientId == item.ClientId && o.ProductId == item.ProductId))
                        {
                            context.OrderItems.Add(item);
                        }
                    }

                    await context.SaveChangesAsync();
                }
            }
        }
    }
