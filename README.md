## GIT LFS Caching Server

This is a server which will act as a proxy to a GIT server and will provide GIT Large File System (LFS) caching support.

The application is written in Microsoft MVC Asp.Net Core 2.2, and is configured to be hosted inside a Docker container.

It is configured to use ASP.Net Identities for basic authentication purposes only.

If the request is not related to LFS it will pass through the commands to the specified GIT server.

If the command is a LFS based it will check the local server first for the cached file, if it exists will serve that file instead of the file on the remote GIT server.

This is useful in the cases where you have limited bandwidth or you want to reduce the amount of used GIT LFS data on GIT providers such as GitHub where GIT LFS has a quota.

## How do I compile?
1. [Install Docker](https://www.docker.com/products/docker-desktop)
2. Open Visual Studio 2017
3. Build
4. Go and debug

The application will debug through the docker automatically.

You can then debug and set breakpoints.

## How do I deploy 

Follow the [Docker Deploy Guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/visual-studio-tools-for-docker?view=aspnetcore-2.2#publish-docker-images)

## How do I configure the server?
1. When you go to the main webpage, create a account.
2. Go to GIT Hosts up the top menu bar.
3. Click "Create New"
4. For the **host URL** put the host without the repository name in it. Eg for GitHub use https://github.com/<your organisation>
5. For the **Host Authentication Token** we use Personal Access Token based login, in GitHub to create a Token go to:
   1. Click your profile picture at the top right of the page.
   2. Click "Settings"
   3. Click "Developer settings"
   4. Click "Personal Access Tokens"
   5. Click "Generate New Token"
   6. Give a name to your token such as "GIT LFS Cache"
   7. Give the application "Repo" rights. 
6. For the **Host Name** give a user friendly name. (This is not used to access the server)
7. For the **Host User Name** give your user name. (This is not used to access the server)
8. Take note of the Git Host ID (numeric number) after you created. This will be used in your GIT url in the next section.

## How do I setup GIT
1. Find your deployed docker's HTTP port.
2. Set the remote for your GIT repository to http://{docker ip}:{docker port}/api/{Git Host ID}/{Repository Name}
3. Eg ```git clone http://localhost:8080/api/1/MyTestRepository```


   
