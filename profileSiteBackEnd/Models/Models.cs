using Microsoft.Extensions.Hosting;

namespace profileSiteBackEnd.Models
{
    public static class SampleData
    {
        #region <seed data>
        public static Profile GetProfile() => new()
        {
            Name = "Albert Campos",
            Title = "Software Developer",
            Tagline = "Full‑stack .NET developer shipping high‑impact systems across legal tech and finance.",
            Summary = "I’m a business-minded software developer with 6+ years building scalable .NET services and React/Vue apps in finance and legal tech. I turn complex requirements into clean, reliable systems—optimizing SQL, automating delivery with Azure DevOps, and partnering closely with product, QA, and infra to ship faster with less toil. Recent wins include cutting database latency ~40% and reducing manual case processing ~30% through workflow automation.",
            Location = "San Antonio, TX",
            Email = "acampos892@gmail.com",
            Github = "https://github.com/albert465",
            Linkedin = "https://www.linkedin.com/in/albert-campos/",
            Skills = new()
        {
            // Backend & Languages
            "C#", ".NET 6–9", "ASP.NET Core", "Entity Framework", "Dapper", "LINQ", "T‑SQL", "PowerShell",
            // Frontend
            "React", "Vue", "TypeScript", "JavaScript", "HTML5", "CSS3", "Bootstrap",
            // Cloud & DevOps
            "Azure App Services", "Azure Pipelines", "Azure SQL", "AKS", "Azure Functions", "AWS EC2/IIS", "Docker", "CI/CD", "IaC",
            // APIs & Architecture
            "REST APIs", "Microservices", "Swagger/OpenAPI", "System Integration",
            // Databases & Reporting
            "SQL Server", "Stored Procedures", "SSIS", "SSRS", "Performance Tuning",
            // Tools & Practices
            "Git", "Azure DevOps", "Visual Studio", "JIRA", "Salesforce", "Agile/Scrum", "SDLC", "TDD", "OOP", "MVVM", "Unit Testing"
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
            Slug = "phi-redactor",
            Title = "PHI Redaction App",
            Description = "C#/.NET service that identifies and redacts PHI from lab orders.",
            Tech = new(){".NET 8", "Regex", "Azure"},
            RepoUrl = null,
            LiveUrl = null,
            Highlights = new(){"Selective redaction by field","Batch file processing","Export sanitized outputs"}
        },
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
            LiveUrl = null,
            Highlights = new(){
                "Accelerated case processing by ~30%",
                "Throughput >500 cases/month"
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
            Published = DateTime.UtcNow.AddDays(-18)
        },
        new Post
        {
            Slug = "ci-cd-azure-devops",
            Title = "CI/CD in Azure DevOps: Practical Patterns",
            Excerpt = "Pipelines, approvals, and safe deployments.",
            Published = DateTime.UtcNow.AddDays(-7)
        }
    };

        public static List<Experience> GetExperience() => new()
    {
        new Experience
        {
            Company = "Easy Expunctions",
            Role = "Software Developer",
            Location = "San Antonio, TX",
            Start = new DateTime(2022,7,1),
            End = new DateTime(2025,2,28),
            Highlights = new()
            {
                "Led end‑to‑end dev of legal automation platforms in .NET 8 + EF + React/Vue, supporting thousands of users.",
                "Delivered 15+ features with legal SMEs, reducing manual processing by ~30%.",
                "Built/optimized SPAs; cut avg load time by ~40% and improved mobile responsiveness.",
                "Refactored legacy code & T‑SQL; improved DB performance by ~40% and reduced errors.",
                "Automated Azure DevOps pipelines; release time down ~60% with zero‑downtime pushes.",
                "Championed code reviews/SDLC best practices to reduce post‑release defects and improve onboarding."
            },
            Tech = new(){ ".NET 8", "EF Core", "React", "Vue", "Azure DevOps", "SQL Server" }
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
                "Led lifecycle dev of ASP.NET internal tools impacting 500+ employees.",
                "Built complex stored procedures & ETL (SSIS); improved data processing and reporting accuracy.",
                "Drove Agile ceremonies; aligned user stories/AC for on‑time delivery.",
                "Developed SSRS reports for leadership; enabled data‑driven decisions.",
                "Standardized Git workflows; reduced merge conflicts by ~30%."
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
                "Resolved complex LMS cases with ~98% SLA adherence and high client satisfaction.",
                "Shipped front‑end enhancements (HTML5/CSS/JS) reducing UI‑related tickets by ~15%.",
                "Improved documentation/workflows, cutting average resolution time by ~10%."
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
                "Integrated SAML/LDAP for 20+ enterprise clients for secure SSO.",
                "Implemented client technical configs for seamless LMS setup and operation."
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
    }

    public class Post
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
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

}
