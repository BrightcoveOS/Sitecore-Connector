
if (document.currentScript != null) {
    //If Context Editor
    var isEditor = window.location.pathname.startsWith("/sitecore/shell/Controls/Rich%20Text%20Editor/");

    //If Experience Editor
    if (new URL(window.location).searchParams.get("sc_mode") == "edit") {
        isEditor = true;
    }

    if (!isEditor) {
        let searchParams = new URL(document.currentScript.src).searchParams;
        let playerId = searchParams.get("player");
        let accountId = searchParams.get("account");
        let playerScript = document.createElement("script");

        if (playerId == null) {
            playerId = "default";
        }

        if (accountId != null) {
            playerScript.setAttribute("src", "https://players.brightcove.net/" + accountId + "/" + playerId + "_default/index.min.js");
            playerScript.setAttribute("type", "text/javascript");

            document.currentScript.parentNode.appendChild(playerScript);
        }
    }
}