# Release synchronization

1. Implement and review the change; preserve existing mod/item IDs and saves.
2. Update the manifest version, all three-language READMEs, native localization and Workshop descriptions together. Retain repository, latest-download and Workshop links.
3. Run `./build.ps1 -GamePath "<Probably Stolen installation>" -Tests`; inspect native scan results and the ZIP under `dist/`.
4. Commit and push the reviewed source, documentation and installable package to GitHub. Publish the matching version tag and Release ZIP, and verify its checksum after downloading.
5. Update the existing [Steam Workshop item](https://steamcommunity.com/sharedfiles/filedetails/?id=3841987033), including its content and localized metadata. Do not create a duplicate item. Keep the Chinese/English cover current when its text changes.
6. Read back all three Workshop language variants and verify both platforms show the same version and working links.

Repository: https://github.com/IEmaster/WaterMerchantGraduate

Latest release download: https://github.com/IEmaster/WaterMerchantGraduate/releases/latest

Documentation-only edits do not change gameplay code. If documentation inside an existing installable ZIP is updated, synchronize the corresponding GitHub download and Workshop content too.
