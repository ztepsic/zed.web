# Zed.Web

[![Zed.Web on nuget.org](https://img.shields.io/nuget/v/Zed.Web.svg)](https://www.nuget.org/packages/Zed.Web) [![Zed.Web on fuget.org](https://www.fuget.org/packages/Zed.Web/badge.svg)](https://www.fuget.org/packages/Zed.Web)

Zed.Web is a library for Asp.Net Core

## CI build status

| Branch  | Build status                                                                                                                                                     |
| ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Master  | [![Main](https://github.com/ztepsic/zed.web/actions/workflows/main.yml/badge.svg)](https://github.com/ztepsic/zed.web/actions/workflows/main.yml)                |
| Develop | [![Main](https://github.com/ztepsic/zed.web/actions/workflows/main.yml/badge.svg?branch=develop)](https://github.com/ztepsic/zed.web/actions/workflows/main.yml) |

## Dev setup

### [Conventional commits](https://www.conventionalcommits.org/)

- [Commit Lint - Lint commit messages](https://commitlint.js.org/)
  - Intall locally
    ```sh
        npm install --save-dev husky
        npx husky init
        echo "npx --no -- commitlint --edit \$1" > .husky/commit-msg
    ```
- [Commitizen](http://commitizen.github.io/cz-cli/)
  - Install locally

    ```sh
    npm install --save-dev commitizen
    npm install --save-dev @commitlint/cz-commitlint commitizen inquirer@9
    ```

### Versioning with [GitVersion](https://gitversion.net/)

- Install .net global tool

  ```sh
  dotnet tool install --global GitVersion.Tool
  ```

- Run

  ```sh
  dotnet-gitversion
  ```

- [Arguments](https://gitversion.net/docs/usage/cli/arguments)

### Mutation testing with Stryker.NET

- Restore local tools

  ```sh
  dotnet tool restore
  ```

- Run mutation testing for changes in your current branch relative to `master`

  ```sh
  cd Zed.Web.Tests
  dotnet tool run dotnet-stryker -- --configuration Release --since:master --break-at 80 --threshold-low 80 --threshold-high 80 --reporter json --reporter html --reporter progress --output ../StrykerOutput/local
  ```
