
/* We cant let videos embedded in rich text fields load while in the content/experience editor 
 * because it modifies the value of the rich text field breaking the embed. Note this only happens
 * if use the rich text editor. Instead we load this script first and check if we are in the 
 * content/experience editor.
 */

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

            document.currentScript.appendChild(playerScript);
        }
    }
}