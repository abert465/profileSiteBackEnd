using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace profileSiteBackEnd.Models
{
    public static class SampleData
    {
        #region <seed data>
        public static Profile GetProfile() => new()
        {
            Name = "Albert Campos",
            Title = "Software Developer",
            Tagline = "Full‑stack .NET developer shipping systems for finance, legal tech, and municipal government.",
            Summary = "Full-stack .NET developer with 6+ years building web applications and business systems for finance, legal tech, and municipal government. I ship end to end on C#, .NET 8/10, ASP.NET Core, and SQL Server, with React, Vue, and Blazor on the front end. Most of my work has been replacing manual process with automation — legal case workflows, deployment pipelines, operational reporting — while keeping the legacy systems they replace running in production.",
            Location = "San Antonio, TX",
            Email = "acampos892@gmail.com",
            Github = "https://github.com/abert465",
            Linkedin = "https://www.linkedin.com/in/albert-campos/",
            Skills = new()
        {
            // Backend & Languages
            // Plain ASCII hyphen in "T-SQL": a non-breaking hyphen (U+2011) here
            // looked identical on screen but seeded a second, duplicate chip.
            "C#", ".NET 6-10", "ASP.NET Core", "Blazor", "Razor Pages", "Entity Framework", "Dapper", "LINQ", "VB.NET", "T-SQL", "PowerShell",
            // Frontend
            "React", "Vue", "TypeScript", "JavaScript", "HTML5", "CSS3", "Bootstrap",
            // Cloud & DevOps
            "Azure App Services", "Azure Pipelines", "Azure SQL", "AKS", "Azure Functions", "AWS EC2/IIS", "Docker", "CI/CD", "IaC",
            // APIs & Architecture
            "REST APIs", "Microservices", "Swagger/OpenAPI", "System Integration",
            // Databases & Reporting
            "SQL Server", "Stored Procedures", "SSIS", "SSRS", "Performance Tuning",
            // Tools & Practices
            "Git", "Azure DevOps", "Visual Studio", "JIRA", "Salesforce", "Power Automate", "Agile/Scrum", "SDLC", "TDD", "OOP", "MVVM", "Unit Testing"
        },
            Links = new() {
            new Link{ Label = "GitHub", Url = "https://github.com/abert465"},
            new Link{ Label = "LinkedIn", Url = "https://www.linkedin.com/in/albert-campos/"}
        }
        };

        public static List<Project> GetProjects() => new()
    {
        new Project
        {
            Slug = "legal-automation",
            Title = "Legal Automation Platform",
            Description = "Workflow engine for expunction processes with React front-end.",
            Tech = new(){".NET", "React", "SQL Server"},
            RepoUrl = null,
            LiveUrl = null,
            Highlights = new(){"Reduced manual steps by 60%","Optimized T-SQL by 40%"}
        },
         new Project
        {
            Slug = "ai-legal-assistant-bot",
            Title = "AI Legal Assistant Bot",
            Description = "Azure Bot chatbot to automate FAQs and reduce attorney workload.",
            Tech = new(){"Azure Bot Service", ".NET", "Azure Functions"},
            RepoUrl = null,
            LiveUrl = null,
            Highlights = new(){
                "Reduced attorney workload by ~40%",
                "Saved 15–20 staff hours/week"
            }
        },
        new Project
        {
            Slug = "enterprise-cloud-migration",
            Title = "Enterprise Cloud Migration",
            Description = "Migration of IIS‑hosted apps from AWS to Azure App Services.",
            Tech = new(){"IIS", "Azure App Services", "CI/CD"},
            RepoUrl = null,
            LiveUrl = null,
            Highlights = new(){
                "20% hosting cost savings",
                "Zero‑downtime deploys via pipelines"
            }
        },
        new Project
        {
            Slug = "automated-expunction-engine",
            Title = "Automated Expunction Engine",
            Description = "Microservice for expunction processing with advanced SQL validation and rules automation.",
            Tech = new(){".NET", "SQL Server", "Microservices"},
            RepoUrl = null,
            LiveUrl = "https://www.easyexpunctions.com/",
            Highlights = new(){
                "Accelerated case processing by ~30%",
                "Throughput >500 cases/month"
            }
        },
        new Project
        {
            Slug = "developer-portfolio-platform",
            Title = "Developer Portfolio Platform",
            Description = "This site: a .NET 10 API with EF Core and SQLite behind a React SPA served same‑origin from wwwroot.",
            Tech = new(){ ".NET 10", "EF Core", "SQLite", "React", "Docker" },
            RepoUrl = "https://github.com/abert465/profileSiteBackEnd",
            LiveUrl = null,
            Highlights = new(){
                "Admin panel over the full content model, with BCrypt auth",
                "Single container: SPA and API share an origin, so no CORS layer"
            }
        }
    };

        public static List<Post> GetPosts() => new()
    {
        new Post
        {
            Slug = "optimizing-tsql",
            Title = "How I Optimized a Critical T‑SQL Stored Procedure by 40%",
            Excerpt = "Index tuning, sargability, and measured rollouts.",
            Published = DateTime.UtcNow.AddDays(-18),
            Content = "When I joined the legal automation platform team, one stored procedure sat at the center of case processing — and at peak load it was the single biggest source of timeouts. Here's how I brought it from a multi-second bottleneck down to something that felt instant.\n\n## Finding the real cost\n\nQuery plans lie if you only look at estimated cost. I pulled actual execution plans under production-like data volume and found two problems: a non-sargable WHERE clause (wrapping an indexed date column in a function, which killed index usage) and an implicit conversion between a VARCHAR parameter and an NVARCHAR column, forcing a full scan on every call.\n\n## Fixing sargability\n\nRewriting the predicates so SQL Server could seek instead of scan was the single biggest win. Instead of filtering on `CONVERT(date, CreatedAt) = @date`, I filtered on a range: `CreatedAt >= @date AND CreatedAt < DATEADD(day, 1, @date)`. Same result set, but now the optimizer could actually use the index.\n\n## Matching types end to end\n\nThe implicit conversion was quieter but just as expensive — invisible in the query text, visible only in the execution plan as an unexpected scan. Aligning the parameter type to the column type let the seek kick in everywhere that predicate was used, not just in this one procedure.\n\n## Measured rollout\n\nI didn't ship this on faith. I benchmarked before/after on a copy of production data, deployed behind the existing CI/CD pipeline, and watched real latency metrics post-release rather than just trusting the query plan. End result: ~40% reduction in execution time, and the timeout errors tied to this procedure dropped to zero.\n\nThe lesson that stuck with me: most SQL performance problems aren't about clever tricks, they're about making sure the optimizer can actually use the indexes you already built."
        },
        new Post
        {
            Slug = "ci-cd-azure-devops",
            Title = "CI/CD in Azure DevOps: Practical Patterns",
            Excerpt = "Pipelines, approvals, and safe deployments.",
            Published = DateTime.UtcNow.AddDays(-7),
            Content = "Moving a legal automation platform to a real CI/CD pipeline in Azure DevOps cut our release time by roughly 60% and got us to zero-downtime deploys. None of it was exotic — it was a handful of practical patterns applied consistently.\n\n## Pipeline as code, not click-ops\n\nEvery pipeline lived in YAML, checked into the repo alongside the code it built. That meant pipeline changes went through the same PR review as application code, and a broken pipeline was debuggable the same way as a broken feature.\n\n## Build once, deploy many times\n\nThe same build artifact moved through dev, staging, and production — no environment-specific rebuilds. This closed off an entire class of \"works in staging, breaks in prod\" bugs caused by dependency drift between builds.\n\n## Approvals where they matter, automation everywhere else\n\nAutomated gates (unit tests, integration tests, static analysis) ran on every push with no human in the loop. Manual approval was reserved for the one step that actually needed a human: promoting to production. That kept the pipeline fast without giving up control over what reached users.\n\n## Zero-downtime by default\n\nDeploys used slot swapping on Azure App Services — deploy to a staging slot, run smoke tests against it, then swap. If the swap revealed a problem, swapping back was just as fast as swapping forward, so rollback was never a scramble.\n\n## What it added up to\n\nRelease time dropped about 60%, and deploys stopped being an event anyone dreaded. The biggest shift wasn't tooling, it was treating deployment as a routine, low-risk action instead of a rare, high-stakes one."
        }
    };

        public static List<Experience> GetExperience() => new()
    {
        new Experience
        {
            Company = "Extra Duty Solutions / Jivasoft",
            Role = "Software Developer",
            Location = "San Antonio, TX",
            Start = new DateTime(2025,9,17),
            End = null, // Present
            Highlights = new()
            {
                "Build full‑stack overtime and scheduling features in .NET 8, C#, Blazor, and SQL Server for police department workforce management, including the Boston Police Department deployment.",
                "Write the T‑SQL stored procedures behind reporting, filtered search, and CRUD operations across the platform.",
                "Implement configuration‑driven role‑based access control so new municipal clients onboard without code changes.",
                "Maintain and extend legacy VB.NET/ASP.NET applications alongside the new platform, shipping fixes and client‑requested enhancements."
            },
            Tech = new(){ ".NET 8", "C#", "Blazor", "SQL Server", "T-SQL", "VB.NET", "ASP.NET" }
        },
        new Experience
        {
            Company = "Easy Expunctions",
            Role = "Software Developer",
            Location = "San Antonio, TX",
            Start = new DateTime(2022,7,1),
            End = new DateTime(2025,2,28),
            Highlights = new()
            {
                "Led end‑to‑end development of a legal automation platform on .NET 8, Entity Framework, and React/Vue, serving thousands of users through court expunction workflows.",
                "Delivered 15+ features with legal subject‑matter experts, automating review steps that previously ran by hand and cutting manual case processing roughly 30%.",
                "Refactored legacy T‑SQL and indexing strategy, cutting database latency about 40% and clearing the timeout errors behind the platform's most frequent support tickets.",
                "Rebuilt release on Azure DevOps pipelines, moving deploys from a scheduled manual event to a zero‑downtime push, and set the code review and SDLC practice the team ran on.",
                "Built Salesforce solutions in Apex and Visualforce with SOQL/SOSL data access, plus Flow automation and integrations through Salesforce APIs, Power Automate, and Azure Functions."
            },
            Tech = new(){ ".NET 8", "EF Core", "React", "Vue", "Azure DevOps", "SQL Server", "Salesforce" }
        },
        new Experience
        {
            Company = "IBC Bank",
            Role = "Software Developer",
            Location = "San Antonio, TX",
            Start = new DateTime(2019,12,1),
            End = new DateTime(2022,7,1),
            Highlights = new()
            {
                "Owned full lifecycle development of internal banking tools in C# and ASP.NET used by 500+ employees across daily operations.",
                "Built SQL Server stored procedures and SSIS ETL pipelines feeding reporting over large transactional datasets.",
                "Developed SSRS reports giving leadership direct visibility into operational banking metrics.",
                "Standardized Git branching and code review across the development team, cutting merge conflicts roughly 30%."
            },
            Tech = new(){ "ASP.NET", "C#", "SQL Server", "SSIS", "SSRS", "Git" }
        },
        new Experience
        {
            Company = "Inspired eLearning",
            Role = "Tier 3 Technical Support Analyst",
            Location = "San Antonio, TX",
            Start = new DateTime(2018,8,1),
            End = new DateTime(2019,12,1),
            Highlights = new()
            {
                "Resolved escalated Learning Management System cases at 98% SLA adherence for enterprise clients.",
                "Shipped HTML5, CSS, and JavaScript front‑end fixes that removed a recurring class of UI support tickets at the source.",
                "Rewrote internal documentation and support workflows, cutting average resolution time about 10%."
            },
            Tech = new(){ "HTML5", "CSS", "JavaScript" }
        },
        new Experience
        {
            Company = "Inspired eLearning",
            Role = "Technical Support Analyst",
            Location = "San Antonio, TX",
            Start = new DateTime(2017,4,1),
            End = new DateTime(2018,8,1),
            Highlights = new()
            {
                "Integrated SAML and LDAP single sign‑on for 20+ enterprise clients.",
                "Owned client technical configuration through go‑live for diverse LMS environments."
            },
            Tech = new(){ "SAML", "LDAP" }
        },
        new Experience
        {
            Company = "Chase Bank",
            Role = "Operations Sr. Specialist",
            Location = "San Antonio, TX",
            Start = new DateTime(2014,11,1),
            End = new DateTime(2017,1,1),
            Highlights = new()
            {
                "Conducted operational audits; implemented process improvements raising productivity ~15%."
            },
            Tech = new()
        },
        new Experience
        {
            Company = "Chase Bank",
            Role = "Technology Operations Intern",
            Location = "San Antonio, TX",
            Start = new DateTime(2013,7,1),
            End = new DateTime(2014,11,1),
            Highlights = new()
            {
                "Resolved 1,000+ Tier 1 cases (VOIP, VDI, desktop) while exceeding resolution targets."
            },
            Tech = new(){ "Windows", "VDI" }
        }
    };
        public static List<Education> GetEducation() => new()
    {
        new Education
        {
            School = "Western Governors University",
            Degree = "B.S. in Software Development (In Progress; Expected 2027)",
            Start = new DateTime(2024, 1, 1),
            End = new DateTime(2027, 12, 1),
            Details = new(){ "Focus: .NET, data structures, databases, SDLC" }
        }
    };
        public static List<Certification> GetCertifications() => new()
    {
        new Certification { Name = "ITIL® Foundation", Issuer = "AXELOS", Issued = null, Expires = new DateTime(2026, 11, 30) },
        new Certification { Name = "Argo Browser‑Based Developer", Issuer = "Argo", Issued = new DateTime(2021, 10, 1), Expires = new DateTime(2022, 11, 1) },
        new Certification { Name = "Software Development Bootcamp", Issuer = "Austin Coding Academy", Issued = new DateTime(2016, 6, 1) }
    };

        // Deliberately empty. This list previously held invented endorsements
        // from people who do not exist, which would have been published on a
        // site recruiters read. Real testimonials are entered through the admin
        // panel; seeding must never manufacture them.
        public static List<Testimonial> GetTestimonials() => new();
        #endregion
    }
    public class Profile
    {
        public int Id { get; set; } = 1;
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Tagline { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Location { get; set; }
        public string? Email { get; set; }
        public string? Github { get; set; }
        public string? Linkedin { get; set; }
        [NotMapped]
        [JsonIgnore]
        public List<string> Skills { get; set; } = new();
        public List<Link> Links { get; set; } = new();
    }

    public class Link
    {
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class Project
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Tech { get; set; } = new();
        public string? RepoUrl { get; set; }
        public string? LiveUrl { get; set; }
        public List<string> Highlights { get; set; } = new();
        public string? ImageUrl { get; set; }
    }

    public class Post
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string? Content { get; set; }
        public DateTime Published { get; set; }
    }
    public class Experience
    {
        public int Id { get; set; } // Auto-incremented primary key
        public string Company { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; } // null for Present
        public List<string> Highlights { get; set; } = new();
        public List<string> Tech { get; set; } = new();
    }
    public class Education
    {
        public int Id { get; set; } // Auto-incremented primary key
        public string School { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<string>? Details { get; set; }
    }

    public class Certification
    {
        public int Id { get; set; } // Auto-incremented primary key
        public string Name { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public DateTime? Issued { get; set; }
        public DateTime? Expires { get; set; }
    }

    public class  Skill
    {
        public int Id { get; set; } // Auto-incremented primary key
        public string Name { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true; // Default to visible
        public int? Order { get; set; } // Default order for sorting

        public int ProfileId { get; set; } // Foreign key to Profile
        public Profile? Profile { get; set; } = null; // Navigation property
    }

    public class Testimonial
    {
        public int Id { get; set; } // Auto-incremented primary key
        public string Name { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public int? Rating { get; set; } // Optional 1-5 rating
        public bool IsVisible { get; set; } = true;
        public int? Order { get; set; } // For display ordering
    }

}
