# Azure DevOps & GitHub Repository Setup Guide for DHCW Test Tools

This guide details the steps and commands to:
1. Initialize a local Git repository (if not already done)
2. Add and push the project to a new GitHub repository
3. Add and push the project to an Azure DevOps repository

---

## 1. Prerequisites
- [Git](https://git-scm.com/downloads) installed
- GitHub account and Azure DevOps account
- Azure DevOps organization and project created
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (optional, for some automation)

---

## 2. Initialize Local Git Repository
Open PowerShell in your project root (`TestTools`):

```powershell
cd "C:\Users\to080141\source\Workspaces\TestTools"
git init
git add .
git commit -m "Initial commit"
```

---

## 3. Create a New GitHub Repository and Push
1. Go to [GitHub](https://github.com/new) and create a new repository (e.g., `TestTools`).
2. Copy the repository URL (e.g., `https://github.com/yourusername/TestTools.git`).
3. Add the remote and push:

```powershell
git remote add origin https://github.com/yourusername/TestTools.git
git branch -M main
git push -u origin main
```

---

## 4. Create a New Azure DevOps Repository and Push
1. Go to your Azure DevOps organization and project.
2. Create a new repository (e.g., `TestTools`).
3. Copy the repository URL (e.g., `https://dev.azure.com/yourorg/yourproject/_git/TestTools`).
4. Add the Azure DevOps remote and push:

```powershell
git remote add azure https://dev.azure.com/yourorg/yourproject/_git/TestTools
git push -u azure main
```

If you already have an `origin` remote (GitHub), you can push to both remotes:

```powershell
git push origin main
git push azure main
```

---

## 5. (Optional) Set Up Azure DevOps CI/CD Pipeline
- In Azure DevOps, go to Pipelines > Create Pipeline
- Connect to your repository and follow the wizard
- Use the provided YAML template or customize as needed

---

## 6. Troubleshooting
- If you get authentication errors, ensure you are signed in with the correct credentials (use `git credential-manager` or Personal Access Tokens for Azure DevOps and GitHub).
- If pushing to Azure DevOps fails, check repository permissions and URL.

---

## References
- [GitHub Docs: Adding a local repo to GitHub](https://docs.github.com/en/get-started/quickstart/create-a-repo)
- [Azure DevOps Docs: Push to Azure Repos](https://learn.microsoft.com/azure/devops/repos/git/pushing)
- [Azure DevOps Docs: Create your first pipeline](https://learn.microsoft.com/azure/devops/pipelines/get-started-yaml)
