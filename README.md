# :world_map: TerrainMap

<div align="center">
    <img alt="GitHub last commit" src="https://img.shields.io/github/last-commit/aiden2480/terrain-map?color=red">
    <img alt="Chrome Web Store Rating" src="https://img.shields.io/chrome-web-store/rating/gmchfepajkbnjgfmomdghcgobhideike?color=orange">
    <img alt="Chrome Web Store Version" src="https://img.shields.io/chrome-web-store/v/gmchfepajkbnjgfmomdghcgobhideike?color=yellow">
    <img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/aiden2480/terrain-map/publish-extension.yml">
    <img alt="Chrome Web Store Users" src="https://img.shields.io/chrome-web-store/users/gmchfepajkbnjgfmomdghcgobhideike?color=blue">
</div>

## :runner: Quickly view & approve Scouts Terrain submissions

Pending approvals can be easily retrieved and actioned, providing an easy interface to enumerate and review submissions. Approvals that you can action have a notification badge next to their icon. Click each panel to expand the submission and see the answers the member has supplied. At the bottom, enter an optional comment, and press improve, or approve.

> [!TIP]
> [Download TerrainMap on the Chrome Webstore](https://chromewebstore.google.com/detail/terrainmap/gmchfepajkbnjgfmomdghcgobhideike) for automatic updates

## :gear: Development Installation
1. `git clone https://github.com/aiden2480/terrain-map`
2. `cd terrain-map && dotnet build TerrainMap`
3. Navigate to `chrome://extensions`
4. Enable `Developer mode`
5. Click `Load unpacked`
6. Select the folder `terrain-map\TerrainMap\bin\Debug\net8.0\browserextension`
