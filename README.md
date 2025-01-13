# ![Icon](doc/logo.png) PokerTime

Remote planning poker tool built in ASP.NET Core and Blazor

Licensed: GNU GPL v3.0

## Features

- Realtime planning poker app, ideal for remote teams
- Create password protected planning poker sessions
- As facilitator, lead the planning poker session through the discussion, estimation and consolidation stages
- Easy to use

### Browser Support

Developed and tested on:

- Microsoft Edge
- Google Chrome
- Mozilla Firefox
- Opera / Opera GX

## Download

### Docker

For further configuration you may want to mount a directory with [the configuration](doc/Installation.md#Configuration):

    docker run -p 80:80 -v /path/to/my/configuration/directory:/etc/pokertime pokertime

### Manual installation

Download the release for your OS from the [releases tab](https://github.com/ennerperez/PokerTime/releases) or download the cutting edge builds from [AppVeyor](https://ci.appveyor.com/project/ennerperez/PokerTime).

[Follow the installation instructions](doc/Installation.md) in the documentation to install it.

## Building PokerTime from sources

If you prefer to build the application yourself, please follow the [compilation instructions](doc/Building-from-sources.md) in the documentation.

## Screenshots

**Create a planning poker session**

![Create a planning poker session](doc/create-session.png)

**Join a planning poker session**

![Join a planning poker session](doc/join-poker-session.png)

**Discussion stage**

![Discussion stage](doc/discussion.png)

**Estimation**
![Estimation](doc/estimation.png)

**Coming to a consensus**
![Estimation discussion](doc/estimation-discussion.png)

**Session conclusion**
![Conclusion](doc/finished.png)

## Contributions

Contributions are allowed and encouraged. In general the rules are: same code style (simply use the included `.editorconfig`), and write automated tests for the changes.

Please submit an issue to communicate in advance to prevent disappointments.

## Attribution

Application icon:

- Icon made by [Eucalyp](https://www.flaticon.com/authors/eucalyp) from [www.flaticon.com](http://www.flaticon.com/)

Built on:

- [Bootstrap](https://getbootstrap.com/) _CSS framework_;
- [Fontawesome](https://fontawesome.io/) as _icon framework_;
- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/) (Blazor Server) with [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for _server side logic and data persistence_;
