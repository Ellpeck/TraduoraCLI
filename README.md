# TraduoraCLI
Command-line interface for importing and exporting translations to [traduora](https://traduora.com/)

# Installing
TraduoraCLI is a `dotnet` tool, meaning you can install it very easily like so:
```sh
dotnet tool install --global TraduoraCLI
```

# Using
First of all, you have to log in to your traduora instance:
```sh
traduora login -s "https://traduora.example.com" -u email@example.com -p mypassword
```
The authentication token will then be stored in your user profile directory for future use.

## Showing stats
```sh
traduora stats -p "ProjectNameOrId"
```

## Exporting translations
```sh
traduora export -p "ProjectNameOrId" -l en de fr -f jsonnested
```

## Importing translations
```sh
traduora import -p "ProjectNameOrId" -l en de fr -f jsonnested
```

# Configuring
TraduoraCLI takes several optional arguments to modify the way it works. To see a list of all possible verbs and their arguments, simply run
```sh
traduora help
```
