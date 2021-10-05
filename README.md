# daBoot 
(Not deployed) Bug submission and tracking software, built using C# on ASP.NET Core MVC framework.

<kbd>![screenshots](https://github.com/wxo15/daBoot/blob/main/website.gif)</kbd>

## Key Functions and Features
* Users can create projects and invite others to join.
* Roles (Lead, dev and testers) can be given to member with different responsibilities and access.
* Tickets can be submitted by both authenticated and unauthenticated users, to ease bug submission by people outside the project.
* Tickets can be assigned to a dev/lead with a specific deadline. Comments can be posted on the ticket by anyone in the team.
* Peripheral functionalities like dashboard, user search, notifications etc.

## Technology Overview
### Front-end
* Responsive UI design
* ChartJS for dashboard graphics

### Back-end
* MySQL for storage

### Authentication
* GitHub OAuth login and username+password login options. Latter is encrypted with salted hash using SHA256. More information can be found [here](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rfc2898derivebytes?view=net-5.0).

## Project Information
* The initial software design spec can be found [here](https://github.com/wxo15/daBoot/blob/main/daBoot%20Spec.pdf).

## Deployment
* I chose not to deploy due to cost reason. If you'd like to deploy this app, you can replicate the database structure by applying Migrations to a relational database of your choice. More info on EF Core's Migration can be found [here](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli).

After that, manually insert the following values to the corresponding tables:

| Values  | Table |
| ------------- | ------------- |
| Closed, Late, Open, Submitted, Rejected | status  |
| Lead, Dev, Tester  | role  |
| High, Medium, Low | priority |

