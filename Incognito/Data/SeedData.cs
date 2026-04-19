using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IncognitoPAS.Data;
using IncognitoPAS.Models;

namespace IncognitoPAS.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        await RemoveLegacyRoleColumnIfExistsAsync(context);
        await EnsureRowVersionColumnsExistAsync(context);

        // Ensure all roles exist
        string[] roles = { "Student", "Supervisor", "ModuleLeader", "SysAdmin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed demo users for each role
        await SeedUserIfNotExists(userManager, "admin@incognito.ac.lk", "Admin@123456", "System Administrator", "SysAdmin");
        await SeedUserIfNotExists(userManager, "leader@incognito.ac.lk", "Leader@123456", "Dr. Sarah Mitchell", "ModuleLeader");
        await SeedUserIfNotExists(userManager, "supervisor1@incognito.ac.lk", "Super@123456", "Prof. James Wilson", "Supervisor");
        await SeedUserIfNotExists(userManager, "supervisor2@incognito.ac.lk", "Super@123456", "Dr. Emily Chen", "Supervisor");
        await SeedUserIfNotExists(userManager, "student1@incognito.ac.lk", "Student@123", "Alex Thompson", "Student");
        await SeedUserIfNotExists(userManager, "student2@incognito.ac.lk", "Student@123", "Maria Garcia", "Student");
        await SeedUserIfNotExists(userManager, "student3@incognito.ac.lk", "Student@123", "Nimal Perera", "Student");
        await SeedUserIfNotExists(userManager, "student4@incognito.ac.lk", "Student@123", "Anjali Fernando", "Student");
        await SeedUserIfNotExists(userManager, "student5@incognito.ac.lk", "Student@123", "Kasun Jayawardena", "Student");
        await SeedUserIfNotExists(userManager, "student6@incognito.ac.lk", "Student@123", "Dilini Gunasekara", "Student");
        await SeedUserIfNotExists(userManager, "student7@incognito.ac.lk", "Student@123", "Chamath De Silva", "Student");
        await SeedUserIfNotExists(userManager, "student8@incognito.ac.lk", "Student@123", "Tharushi Wickramasinghe", "Student");
        await SeedUserIfNotExists(userManager, "student9@incognito.ac.lk", "Student@123", "Harini Senanayake", "Student");
        await SeedUserIfNotExists(userManager, "student10@incognito.ac.lk", "Student@123", "Ruwan Hettiarachchi", "Student");

        // Seed research areas
        if (!await context.ResearchAreas.AnyAsync())
        {
            var areas = new List<ResearchArea>
            {
                new() { Name = "Artificial Intelligence", Description = "AI, deep learning, neural networks" },
                new() { Name = "Web Development", Description = "Full-stack web applications" },
                new() { Name = "Cybersecurity", Description = "Security, cryptography, ethical hacking" },
                new() { Name = "Cloud Computing", Description = "AWS, Azure, distributed systems" },
                new() { Name = "Machine Learning", Description = "ML algorithms, data analysis" },
                new() { Name = "Data Science", Description = "Big data, analytics, visualization" },
                new() { Name = "Mobile Development", Description = "iOS, Android, cross-platform" },
                new() { Name = "IoT", Description = "Internet of Things, embedded systems" }
            };
            context.ResearchAreas.AddRange(areas);
            await context.SaveChangesAsync();
        }

        // Seed supervisor expertise
        if (!await context.SupervisorExpertises.AnyAsync())
        {
            var super1 = await userManager.FindByEmailAsync("supervisor1@incognito.ac.lk");
            var super2 = await userManager.FindByEmailAsync("supervisor2@incognito.ac.lk");
            var areas = await context.ResearchAreas.ToListAsync();

            if (super1 != null)
            {
                context.SupervisorExpertises.Add(new SupervisorExpertise { SupervisorId = super1.Id, ResearchAreaId = areas[0].Id });
                context.SupervisorExpertises.Add(new SupervisorExpertise { SupervisorId = super1.Id, ResearchAreaId = areas[4].Id });
                context.SupervisorExpertises.Add(new SupervisorExpertise { SupervisorId = super1.Id, ResearchAreaId = areas[5].Id });
            }
            if (super2 != null)
            {
                context.SupervisorExpertises.Add(new SupervisorExpertise { SupervisorId = super2.Id, ResearchAreaId = areas[1].Id });
                context.SupervisorExpertises.Add(new SupervisorExpertise { SupervisorId = super2.Id, ResearchAreaId = areas[6].Id });
                context.SupervisorExpertises.Add(new SupervisorExpertise { SupervisorId = super2.Id, ResearchAreaId = areas[7].Id });
            }
            await context.SaveChangesAsync();
        }

        // Seed sample proposals for demo
        if (!await context.Proposals.AnyAsync())
        {
            var student1 = await userManager.FindByEmailAsync("student1@incognito.ac.lk");
            var student2 = await userManager.FindByEmailAsync("student2@incognito.ac.lk");
            var areas = await context.ResearchAreas.ToListAsync();

            if (student1 != null)
            {
                context.Proposals.Add(new ProjectProposal
                {
                    Title = "Deep Learning for Medical Image Classification",
                    Abstract = "This research proposes developing a convolutional neural network system capable of classifying medical images for early disease detection. The study will explore transfer learning techniques and data augmentation methods to improve model accuracy while reducing the need for large labeled datasets. Expected outcomes include a working prototype and published findings on model efficiency.",
                    TechnicalStack = "Python, TensorFlow, Keras, OpenCV",
                    ResearchAreaId = areas[0].Id,
                    StudentId = student1.Id,
                    Status = ProposalStatus.Pending
                });

                context.Proposals.Add(new ProjectProposal
                {
                    Title = "Real-time Vehicle Detection Using YOLO",
                    Abstract = "This project aims to implement and optimize the YOLO (You Only Look Once) algorithm for real-time vehicle detection in urban traffic scenarios. The research will involve dataset collection, model training, and deployment on edge devices for practical traffic monitoring applications. Performance metrics will be evaluated against existing solutions.",
                    TechnicalStack = "Python, PyTorch, OpenCV, Raspberry Pi",
                    ResearchAreaId = areas[4].Id,
                    StudentId = student1.Id,
                    Status = ProposalStatus.UnderReview
                });
            }

            if (student2 != null)
            {
                context.Proposals.Add(new ProjectProposal
                {
                    Title = "Progressive Web App for Campus Navigation",
                    Abstract = "Developing a cross-platform progressive web application that provides indoor and outdoor navigation for university campuses. The app will feature AR-based directions, real-time POI information, and offline functionality. This project addresses the common problem of getting lost in large campus environments.",
                    TechnicalStack = "React, TypeScript, Firebase, WebXR",
                    ResearchAreaId = areas[1].Id,
                    StudentId = student2.Id,
                    Status = ProposalStatus.Pending
                });

                context.Proposals.Add(new ProjectProposal
                {
                    Title = "IoT-Based Smart Irrigation System",
                    Abstract = "Designing and implementing an automated irrigation system using IoT sensors to monitor soil moisture, weather conditions, and plant health. The system will use machine learning to optimize water usage while maintaining healthy plant growth. A mobile app will provide remote monitoring and control capabilities.",
                    TechnicalStack = "Arduino, ESP32, Python, MQTT, React Native",
                    ResearchAreaId = areas[7].Id,
                    StudentId = student2.Id,
                    Status = ProposalStatus.Matched
                });
            }

            await context.SaveChangesAsync();

            // Add a match for the matched proposal
            if (student2 != null)
            {
                var matchedProposal = await context.Proposals.FirstOrDefaultAsync(p => p.Status == ProposalStatus.Matched);
                var super2 = await userManager.FindByEmailAsync("supervisor2@incognito.ac.lk");
                if (matchedProposal != null && super2 != null)
                {
                    context.SupervisorMatches.Add(new SupervisorMatch
                    {
                        ProposalId = matchedProposal.Id,
                        SupervisorId = super2.Id,
                        IsConfirmed = true,
                        IsRevealed = true,
                        ConfirmedAt = DateTime.UtcNow.AddDays(-1)
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }

    private static async Task RemoveLegacyRoleColumnIfExistsAsync(ApplicationDbContext context)
    {
        const string dropLegacyRoleColumnSql = """
IF COL_LENGTH('dbo.AspNetUsers', 'Role') IS NOT NULL
BEGIN
    DECLARE @constraintName nvarchar(128);

    SELECT @constraintName = dc.name
    FROM sys.default_constraints AS dc
    INNER JOIN sys.columns AS c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.AspNetUsers')
      AND c.name = N'Role';

    IF @constraintName IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE [dbo].[AspNetUsers] DROP CONSTRAINT [' + @constraintName + N']');
    END

    ALTER TABLE [dbo].[AspNetUsers] DROP COLUMN [Role];
END
""";

        await context.Database.ExecuteSqlRawAsync(dropLegacyRoleColumnSql);
    }

    private static async Task EnsureRowVersionColumnsExistAsync(ApplicationDbContext context)
    {
        const string ensureRowVersionColumnsSql = """
IF COL_LENGTH('dbo.Proposals', 'RowVersion') IS NULL
BEGIN
    ALTER TABLE [dbo].[Proposals] ADD [RowVersion] rowversion;
END

IF COL_LENGTH('dbo.SupervisorMatches', 'RowVersion') IS NULL
BEGIN
    ALTER TABLE [dbo].[SupervisorMatches] ADD [RowVersion] rowversion;
END
""";

        await context.Database.ExecuteSqlRawAsync(ensureRowVersionColumnsSql);
    }

    private static async Task SeedUserIfNotExists(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
