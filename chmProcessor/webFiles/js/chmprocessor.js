
// TODO: Add links to print, about, home, previous, next
// TODO: Change tree node expansion animation speed
// TODO: Handle full text search
// TODO: When a title is repeated, save the order number on the hash

var pageLayout; // a var is required because this page utilizes: pageLayout.allowOverflow() method

if (!String.prototype.trim) {
    //code for trim (part of the ECMAScript 5 standard)
    String.prototype.trim = function() { return this.replace(/^\s+|\s+$/g, ''); };
}

// Returns the last index of a character into a string. Returns < 0 if it was not found
// character: Character to search
if (!String.prototype.lastIndexOf) {
    String.prototype.lastIndexOf = function(character) {
        for (i = (this.length - 1); i >= 0; i--) {
            if (this.charAt(i) == character)
                return i;
        }
        return -1;
    }
}

// Returns the file name of a URL
function getUrlFileName(url) {
    idx = url.lastIndexOf('/');
    if (idx >= 0)
        return url.substring(idx + 1);
    else
        return url;
}

// Loads a URL into the current topic iframe
// url: Relative topic URL to load
function loadUrlOnFrame(url) {

    var iframeSelector = $("#mainFrame");
    try {
        // This will fail on chrome with file:// protocol
        var currentUrl = getUrlFileName(iframeSelector.prop("contentWindow").location.href);
        if (currentUrl.toLowerCase() == url.toLowerCase())
            return;
    }
    catch (ex) { }
    //$("#mainFrame").attr("src", url); < This stores an history point. We dont want that
    iframeSelector.prop("contentWindow").location.replace(url);

}

// Select a tree node by its URL
// url: string with the URL to search
function selectByUrl(url) {
    var fileName = getUrlFileName(url);
    fileName = decodeURIComponent(fileName); // Needed if the hash part(xxx on a.html#xxx) contains spaces, it happens with word generated hashes
    // Do tree selection
    var linkSelected = $('#treediv a[href="' + fileName + '"]').first();
    $("#treediv").jstree("select_node", linkSelected.parent(), true);
    // Load the URL on the frame
    loadUrlOnFrame(fileName);
};

// Process a tree link text
function cleanTitleText(linkText) {
    // Remove line breaks and leading spaces
    var cleanText = linkText.replace("\n", " ").trim();
    // Replace extra spaces by a single one
    return cleanText.replace(/\s+/g, " ");
}

// Select a tree node by its title
// title: Tree node title to select
function selectByTitle(title) {

    title = cleanTitleText(title).toLowerCase();

    if (title == "")
        // Select the first node
        $("#treediv").jstree("select_node", $("#treediv li:first"), true);
    else
        // Select the node by title
        $("#treediv").jstree("select_node",
            $("#treediv a")
            .filter(function(index) {
                return cleanTitleText($(this).text()).toLowerCase() == title;
            })
            .first().parent(),
            true
        );
}

// Return the window current hash
function getCurrentHash() {
    // Firefox returns the hash unescaped, so decodeURIComponent fails...
    /*var title = window.location.hash;
    if (title.charAt(0) == '#')
        title = title.substring(1);*/
    var hash = location.href.split("#")[1];
    if (!hash)
        hash = "";
    return hash;
}

// Hash change event handler
function hashChanged() {
    var title = getCurrentHash();
    title = decodeURIComponent(title);
    selectByTitle(title);
}

// Set a new URL hash
// newHash: string with the new hash
function changeHash(newHash) {

    if (!("onhashchange" in window))
        return;

    newHash = cleanTitleText(newHash);
    
    // The first node should no have hash:
    if (cleanTitleText($("#treediv a:first").text()) == newHash)
        newHash = "";

    // Avoid to put the same hash twice
    if (window.location.hash == newHash)
        return;

    newHash = encodeURIComponent(newHash);
    window.location.hash = newHash;
}


$(document).ready(function() {

    // Create the tree
    $("#treediv").jstree({
        // the `plugins` array allows you to configure the active plugins on this instance
        "plugins": ["themes", "html_data", "ui", "hotkeys"],
        // Single selection
        "ui": { "select_limit": 1 }
    })
    // Tree node selection event:
    .bind("select_node.jstree", function(event, data) {
        // `data.rslt.obj` is the jquery extended node that was clicked

        // Get the tree node link
        var link = data.rslt.obj.find("a:first");
        var url = link.attr("href");

        // Load it as the current topic
        loadUrlOnFrame(url);

        // Set the URL hash with the title
        changeHash(link.text());

    })
    // Set initial selection
    .bind("loaded.jstree", function(e, data) {
        if (getCurrentHash())
            // There is an initial hash: Select it
            hashChanged();
        else
            // Select the first node
            $("#treediv").jstree("select_node", $("#treediv li:first"), true)
    });

    // If a link is pressed into the frame, search and select the new URL into the tree:
    $('#mainFrame').load(function() {
        try {
            // This will throw an exception with chrome on local system file
            var url = getUrlFileName($(this).get(0).contentWindow.document.location.href);
            selectByUrl(url);
        }
        catch (ex) { }
    });

    // Create contents tabs
    $(".ui-layout-west").tabs({
        activate: function(event, ui) {
            // Set the focus on the search fields when we change the current tab:
            var tabId = ui.newPanel.attr('id');
            if (tabId == 'tab-index')
                $("#searchTopic").focus();
            else if (tabId == 'tab-search')
                $("#searchText").focus();
        }
    });

    // Create layouts
    pageLayout = $('body').layout({
        west__size: .25
        , center__maskContents: true // IMPORTANT - enable iframe masking
    });

    // Set index events:
    // Topic index textbox:
    $("#searchTopic")
    .keyup(function(e) {
        if (e.which == 13) {
            // Enter was pressed: Load the selected URL:
            selectByUrl($("#topicsList").val());
        }
        else {
            // Select the first list topic starting with the typed text:
            var currentText = $(this).val().toLowerCase();
            $("#topicsList").val(
                $("#topicsList > option")
                .filter(function(index) {
                    return $(this).text().toLowerCase().indexOf(currentText) == 0;
                })
                .first()
                .val()
            );
        }
    });
    // Index listbox:
    $("#topicsList")
    .keyup(function(e) {
        if (e.which == 13)
        // Enter was pressed: Load the selected URL:
            selectByUrl($("#topicsList").val());
    })
    .change(function() {
        // Selected topic changed: Set the topic textbox with the title:
        $("#searchTopic").val($("#topicsList > option:selected").text());
    })
    .click(function() {
        // Load the selected URL:
        selectByUrl($("#topicsList").val());
    });

    // Set text search type:
    // Disable submit if there is nothing to search:
    $("#searchText").keyup(function(e) {
        $("#btnSearch").prop('disabled', $("#searchText").val() == '');
    });
    // Initial check:
    $("#btnSearch").prop('disabled', $("#searchText").val() == '');
    if (fullSearch) {
        // Hide the result list:
        $("#searchResult").remove();
        // TODO: Set the submit destination for the search form
    }
    else {
        // Handle the search form submit event:
        $("#searchform").submit(function(e) {

            var searchText = $("#searchText").val();
            if (searchText == '') {
                // Nothing to search:
                e.preventDefault();
                return;
            }

            // Clear previous search results:
            var searchResultOptions = $("#searchResult").prop("options");
            searchResultOptions.length = 0;

            // Do the search over the tree: All links with the text, case insensitive
            $("#treediv a")
            .filter(function(index) {
                return $(this).text().toLowerCase().indexOf(searchText) >= 0;
            })
            .each(function() {
                // Add an option to the search listbox with the text and the URL of the link:
                searchResultOptions[searchResultOptions.length] =
                    new Option(cleanTitleText($(this).text()), $(this).attr('href'));
            });

            // Cancel submit            
            e.preventDefault();
        });

        $("#searchResult")
        .click(function() {
            // Load the selected URL:
            selectByUrl($("#searchResult").val());
        })
        .keyup(function(e) {
            if (e.which == 13)
            // Enter was pressed: Load the selected URL:
                selectByUrl($("#searchResult").val());
        });

    }

    if ("onhashchange" in window) {
        // Browser supports hash change: Add the event handler
        window.onhashchange = hashChanged;
    }

});

