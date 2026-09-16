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
            // Blank lines are paragraph breaks; About.jsx splits on them. The Hero
            // tagline already states the role and the three industries, so this
            // deliberately opens somewhere else instead of repeating it.
            Summary =
                "Most of what I build replaces a spreadsheet, a paper form, or something a person did by hand every Friday.\n\n" +
                "Right now that's overtime and scheduling for police departments — Boston PD among them — in .NET 8, Blazor, and the T-SQL behind the reports people check every morning. Before that, an expunction pipeline that turned court filings from a manual slog into a workflow and cut manual case processing about 30%, and a platform migration off AWS onto Azure.\n\n" +
                "That last one is most of the job, honestly. Rewrites are easy when nothing is live. Almost everything I've shipped ran beside the system it replaced until the day it didn't, and the win condition is that nobody using it ever noticed the seam. Six years of that across finance, legal tech, and government has made me careful about migrations, boring about data integrity, and quick at reading someone else's code.\n\n" +
                "This site is the same idea at small scale — .NET 10 API, React SPA, one box, Cloudflare Tunnel in front. I broke it twice getting here. Currently open to full-stack .NET roles.",
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
            "Git", "Azure DevOps", "Visual Studio", "JIRA", "Salesforce", "Power Automate", "Agile/Scrum", "SDLC", "TDD", "OOP", "MVVM", "Unit Testing", "Code Review"
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
            SortOrder = 4,
            ImageUrl = "/uploads/projects/enterprise-cloud-migration.svg",
            Slug = "enterprise-cloud-migration",
            Title = "AWS to Azure Platform Migration",
            Description = "Moved two .NET applications — one consumer facing, one business facing — off self‑managed IIS on AWS onto Azure App Services across two months, carrying Salesforce and third‑party data provider integrations with them.",
            Tech = new(){"IIS", "AWS", "Azure App Services", "Salesforce API", "CI/CD"},
            RepoUrl = null,
            LiveUrl = null,
            Highlights = new(){
                "Cut over in dependency order inside a maintenance window: database first, then the services the apps depended on, then the apps themselves",
                "Cut hosting costs 20% by consolidating onto a shared App Service plan and shutting down AWS resources that were billing without being used",
                "Kept Salesforce and external data provider integrations live through the move",
                "Most rollbacks traced to configuration rather than code — endpoints and settings that did not follow the apps across"
            }
        },
        new Project
        {
            SortOrder = 2,
            ImageUrl = "/uploads/projects/automated-expunction-engine.jpg",
            Slug = "automated-expunction-engine",
            Title = "Expunction Automation Platform",
            Description = "Workflow engine and processing microservice for legal expunctions, with rules automation, SQL validation, and a React front end.",
            Tech = new(){".NET", "React", "SQL Server", "Microservices"},
            RepoUrl = null,
            LiveUrl = "https://www.easyexpunctions.com/",
            // Figures match the resume exactly. Two documents quoting different
            // numbers for the same work is a question you do not want to field.
            // Merged from the old legal-automation card, which described this
            // same system and restated the 30% figure a second time.
            Highlights = new(){
                "Accelerated case processing by ~30%",
                "Cut database latency about 40%",
                "Throughput >500 cases/month"
            }
        },
        new Project
        {
            SortOrder = 1,
            ImageUrl = "/uploads/projects/police-overtime-scheduling.jpg",
            Slug = "police-overtime-scheduling",
            Title = "Police Overtime Scheduling Portal",
            Description = "Blazor Server replacement for the Boston Police Department's legacy \"Blue Note\" overtime workflow: request intake, hours-based assignment and call-in ordering, and the printed lists the department runs on. Live link is the sign-in page — the portal is department staff only.",
            Tech = new(){ ".NET 8", "Blazor Server", "SQL Server", "OpenID Connect SSO", "Clean Architecture" },
            RepoUrl = null,
            LiveUrl = "https://bostonot.extradutysolutions.com/",
            Highlights = new(){
                "Sole developer: architecture, data layer, UI, SSO, and test suite",
                "Recovered the business rules from the legacy system's training manual — no specification existed — then refined them through department testing",
                "Modernized the clerk and supervisor experience without disturbing the surrounding paper and payroll processes",
                "Single sign-on against the City of Boston OIDC tenant, with no self-registration path",
                "Built to WCAG 2.1 AA, since a public-sector tool has to be usable by everyone on the shift"
            }
        },
        new Project
        {
            SortOrder = 3,
            ImageUrl = "/uploads/projects/naas-nexus-work-orders.jpg",
            Slug = "naas-nexus-work-orders",
            Title = "Aviation MRO Work Order Platform",
            Description = "Blazor Server operations portal in production for an aircraft fuel tank MRO: work order lifecycle, labor budgets, crew scheduling, and a public job postings API consumed by naasllc.com. The live link is the sign-in page — the portal itself is staff-only.",
            Tech = new(){ ".NET 9", "Blazor Server", "ASP.NET Identity", "JWT", "EF Core", "SQL Server" },
            RepoUrl = null,
            // Links to a login wall by design. The card says so up front, since the
            // button just reads "Live" and an unexplained gate looks broken.
            LiveUrl = "https://nexus.naasllc.com/",
            Highlights = new(){
                "Budget engine rolls time entries into per-task actuals and flags overruns before they land",
                "Hosted service promotes work orders to Active on their start date, no manual sweep",
                "Role-scoped access across corporate, manager, and field crew, enforced per location"
            }
        },
        new Project
        {
            SortOrder = 5,
            ImageUrl = "/uploads/projects/developer-portfolio-platform.jpg",
            Slug = "developer-portfolio-platform",
            Title = "Developer Portfolio Platform",
            Description = "This site: a .NET 10 API with EF Core and SQLite behind a React SPA served same‑origin from wwwroot, self‑hosted and published through a Cloudflare Tunnel.",
            Tech = new(){ ".NET 10", "EF Core", "SQLite", "React", "Cloudflare Tunnel" },
            RepoUrl = "https://github.com/abert465/profileSiteBackEnd",
            LiveUrl = "https://tedko.dev",
            Highlights = new(){
                "Admin panel over the full content model, with BCrypt auth",
                "Runs as a Windows service bound to loopback; no inbound port is open",
                "SPA and API share an origin, so there is no CORS layer to configure"
            }
        },
        new Project
        {
            SortOrder = 6,
            // No screenshot yet and both repositories are private, so the card
            // renders the initial-letter placeholder and shows no buttons.
            ImageUrl = null,
            Slug = "smartfit-nutrition-tracker",
            Title = "SmartFit Nutrition Tracker",
            Description = "In-progress nutrition and fitness tracker: a Fastify and TypeScript API over Postgres paired with a React Native client, where a meal can be logged by photo, by barcode, by scanning a nutrition label, or by search.",
            Tech = new(){ "TypeScript", "Fastify", "Prisma", "PostgreSQL", "React Native", "Gemini API" },
            RepoUrl = null,
            LiveUrl = null,
            Highlights = new(){
                "Four ways into a food log — photo estimation through the Gemini API, barcode lookup against Open Food Facts, OCR of nutrition labels with Tesseract, and search over an imported USDA dataset",
                "OpenAPI spec and shared types sit in the same workspace as the API, so the mobile client and the server cannot drift apart quietly",
                "JWT auth with token refresh handled in the client's HTTP layer, rate limiting and per-user authorization enforced server side",
                "User-submitted foods carry a reputation score, so community data can be trusted without hand-moderating every entry"
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
        // The two Inspired eLearning roles and the two Chase roles are each a
        // single entry covering the full span, with the promotion noted in the
        // role line. This matches the resume: a recruiter reading both should
        // not find a different number of jobs or different dates in each.
        new Experience
        {
            Company = "Inspired eLearning",
            Role = "Tier 3 Technical Support Analyst",
            RoleNote = "Promoted from Technical Support Analyst, August 2018",
            Location = "San Antonio, TX",
            Start = new DateTime(2017,4,1),
            End = new DateTime(2019,12,1),
            Highlights = new()
            {
                "Resolved escalated Learning Management System cases at 98% SLA adherence for enterprise clients.",
                "Shipped HTML5, CSS, and JavaScript front‑end fixes that removed a recurring class of UI support tickets at the source.",
                "Integrated SAML and LDAP single sign‑on for 20+ enterprise clients and owned their technical configuration through go‑live."
            },
            Tech = new(){ "HTML5", "CSS", "JavaScript", "SAML", "LDAP" }
        },
        new Experience
        {
            Company = "Chase Bank",
            Role = "Operations Sr. Specialist",
            RoleNote = "Started as Technology Operations Intern",
            Location = "San Antonio, TX",
            Start = new DateTime(2013,7,1),
            End = new DateTime(2017,1,1),
            Highlights = new()
            {
                "Audited daily operational workflows and implemented process improvements that raised team productivity about 15%; resolved 1,000+ Tier 1 VOIP, VDI, and desktop cases as an intern before moving into the specialist role."
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
        // Display order, lowest first. Without this the API fell back to sorting
        // by Title, which buried the strongest work under whatever happened to
        // start with an early letter.
        public int SortOrder { get; set; }
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

        // Progression within a single entry, e.g. "Promoted from Technical
        // Support Analyst, August 2018". Lets one row cover a span that included
        // a title change, instead of splitting it into two entries and making
        // the history look longer than the resume's.
        public string? RoleNote { get; set; }

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
