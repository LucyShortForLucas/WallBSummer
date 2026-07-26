# WallBSummer

## Project repo set up (Windows)

To set up the git repo on your device follow these steps:

1. Install the prerequisite software:
    - Git: https://git-scm.com/install/windows
    - Google Drive for Desktop: https://support.google.com/a/users/answer/13022292?hl=en
    - Rust: https://rust-lang.org/tools/install/

2. Make a Github account: https://github.com/
    - 2b. Send Lucy your github account name and wait for her to add you as a collaborator

3. Set up the GDrive folder:
    1. On the Google Drive website, find the shared GroupProjects2026 folder and make a shortcut to it in 'My drive'
    2. Navigate to the G:\My Drive drive in folder explorer
    3. Find the shared GroupProjects2026 in My Drive and set it to be available offline
        - Right click -> Offline Access -> Available Offline

4. Clone the git repo
    - In the terminal/Powershell, run:
        ``git clone https://github.com/LucyShortForLucas/WallBSummer.git``
    - Alternatively, bother Thommy about how to use a git GUI

5. Install GIT lfs
    1. Run ``cargo install lfs-dal``
    2. Run the ``setup-lfs.ps1`` shell script

6. Add all binary file extensions that need to be on the LFS to ``.gitattributes``