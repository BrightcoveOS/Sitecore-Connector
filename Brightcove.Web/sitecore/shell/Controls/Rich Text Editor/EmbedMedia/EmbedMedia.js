function GetDialogArguments() {
    return getRadWindow().ClientParameters;
}

function getRadWindow() {
    if (window.radWindow) {
        return window.radWindow;
    }

    if (window.frameElement && window.frameElement.radWindow) {
        return window.frameElement.radWindow;
    }

    return null;
}

var isRadWindow = true;

var radWindow = getRadWindow();

if (radWindow) {
    if (window.dialogArguments) {
        radWindow.Window = window;
    }
}

function scClose(media) {
    var returnValue = {
        media: media
    };

    getRadWindow().close(returnValue);

}

function scCancel() {
    getRadWindow().close();
}

function scNext() {
    var editorOptionsContainer = new EditorOptions();
    if (document.getElementById('ShowPlaylistHead').value != '') {
        document.getElementById('VideoHeadContainer').style.display = 'none';
        document.getElementById('PlaylistHeadContainer').style.display = 'block';
    } else {
        document.getElementById('VideoHeadContainer').style.display = 'block';
        document.getElementById('PlaylistHeadContainer').style.display = 'none';
    }
}

function UpdateShortcode(key, value) {
    var shortcodeInput = document.getElementById('ShortcodeInput');

    if (shortcodeInput.value.includes(key)) {
        shortcodeInput.value = shortcodeInput.value.replace(key + '=\'' + /[^(?!.*(\'|\"))]/ + '\'', key + '=\'' + value + '\'');
    }
}

function UpdateHeight(widthInput, widthAspect, heightInput, heightAspect) {
    heightInput.value = Math.ceil((widthInput.value / widthAspect) * heightAspect);
}

function CalculateDimensions(aspectRatioList, widthInput, heightInput) {
    var widthHeight = aspectRatioList.value.split(':');
    var width = '';
    var height = '';
    if (widthHeight.length >= 2) {
        width = widthHeight[0];
        height = widthHeight[1];
    }
    heightInput.disabled = aspectRatioList.value !== 'Custom';
    if (width !== '' && height !== '') {
        UpdateHeight(widthInput, width, heightInput, height);
    }
}
function SetRadioSelection(input, group) {
    for (var i = 0; i < group.length; i++) {
        if (group[i].checked)
            input.value = group[i].value;
    }
}

var EditorOptions = function () {
    var aspectRatioSelect = document.getElementById('AspectRatioList');
    var widthInput = document.getElementById('WidthInput');
    var heightInput = document.getElementById('HeightInput');
    var embedInput = document.getElementById('EmbedInput');
    var sizingInput = document.getElementById('SizingInput');

    var embedRadiogroup = document.getElementsByName("EmbedRadiogroup");
    var sizingRadiogroup = document.getElementsByName("SizingRadiogroup");

    /*for (var i = 0; i < embedRadiogroup.length; i++) {
        embedRadiogroup[i].addEventListener("change", function () { SetRadioSelection(embedInput, embedRadiogroup); }, false);
    }
    */
    for (var i = 0; i < sizingRadiogroup.length; i++) {
        sizingRadiogroup[i].addEventListener("change", function () { SetRadioSelection(sizingInput, sizingRadiogroup); }, false);
    }
    if (aspectRatioSelect !== null && widthInput !== null && heightInput !== null) {
        aspectRatioSelect.addEventListener("change", function () { CalculateDimensions(aspectRatioSelect, widthInput, heightInput); }, false);
        widthInput.addEventListener("change", function () { CalculateDimensions(aspectRatioSelect, widthInput, heightInput); }, false);
    }

    document.getElementById("IframeRadiobutton").addEventListener("change", function (event) {
        document.getElementById("ResponsiveRadiobutton").disabled = true;
        document.getElementById("FixedRadiobutton").checked = true;
        SetRadioSelection(embedInput, embedRadiogroup);
        SetRadioSelection(sizingInput, sizingRadiogroup);
    });

    document.getElementById("JavascriptRadiobutton").addEventListener("change", function () {
        document.getElementById("ResponsiveRadiobutton").disabled = false;
        document.getElementById("ResponsiveRadiobutton").checked = true;
        SetRadioSelection(embedInput, embedRadiogroup);
        SetRadioSelection(sizingInput, sizingRadiogroup);
    });
}