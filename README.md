# TraduoraCLI
Command-line interface for importing and exporting translations to [traduora](https://traduora.com/)

# Installing
TraduoraCLI is a `dotnet` tool, meaning you can install it very easily like so:
```
dotnet tool install --global TraduoraCLI
```

# Using
First of all, you have to log in to your traduora instance:
```
traduora login -s "https://traduora.example.com" -u email@example.com -p mypassword
```
If you want to log in with a client instead, be sure to add the `-c` option. The authentication token will be stored in your user profile directory for future use.

## Showing stats
```
traduora stats -p "ProjectNameOrId"
```

## Exporting translations
```
traduora export -p "ProjectNameOrId" -l en de fr -f jsonnested
```

## Importing translations
```
traduora import -p "ProjectNameOrId" -l en de fr -f jsonnested
```

# Configuring
TraduoraCLI takes several optional arguments to modify the way it works. To see a list of all possible verbs and their arguments, simply run
```
traduora help
```
