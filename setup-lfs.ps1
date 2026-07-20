# setup-lfs.ps1
# Run this once after cloning the repo to configure Git LFS
# to use our Drive-backed storage via lfs-dal.
#
# Prerequisites (install these first if you don't have them):
#   - Git for Windows
#   - Git LFS (git-lfs.com)
#   - Rust + Cargo (rustup.rs), then: cargo install lfs-dal
#   - Google Drive for desktop, with the shared LFS folder
#     set to sync locally (Stream mode + "Available offline"
#     on the LFS folder, or Mirror mode)
#
# If your Drive folder resolves to a different local path than
# the one below, edit the "root" line to match your machine
# before running this script.

git lfs install --local

git config lfs.customtransfer.lfs-dal.path "lfs-dal"
git config lfs.standalonetransferagent lfs-dal

git config -f .lfsdalconfig lfs-dal.scheme fs
git config -f .lfsdalconfig lfs-dal.root "G:\.shortcut-targets-by-id\1DCjk_n6IbwflVbzSh7G2kYXci_Lf2pyX\GroupProjects2026\LFS_SummerPrototype"

Write-Host "Configured lfs-dal as the standalone LFS transfer agent."
Write-Host "Pulling LFS content from Drive..."
git lfs pull

Write-Host ""
Write-Host "Done. Verify by opening an LFS-tracked asset (e.g. a texture) and"
Write-Host "confirming it's the real file, not raw pointer text."
