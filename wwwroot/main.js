function addToArticle(stringToAdd, cursorPosMod)
{
    var textarea = document.getElementById("textareapost");

    // Derived from https://stackoverflow.com/a/11077016/1430304
    //IE support
    if (document.selection)
    {
        textarea.focus();
        sel = document.selection.createRange();
        sel.text = stringToAdd;
    }
    //MOZILLA and others
    else if (textarea.selectionStart || textarea.selectionStart == '0')
    {
        var startPos = textarea.selectionStart;
        var endPos = textarea.selectionEnd;
        textarea.value = textarea.value.substring(0, startPos)
            + stringToAdd
            + textarea.value.substring(endPos, textarea.value.length);

        textarea.selectionStart = startPos + stringToAdd.length + cursorPosMod;
        textarea.selectionEnd = startPos + stringToAdd.length + cursorPosMod;
    } else {
        textarea.value += stringToAdd;
    }

    textarea.focus();
}

var tabEnabled = 0;

// Background of this function: Blind people rely on the tab key for navigation on pages and we don't want to steal that from them.
function enableTab()
{
    if(tabEnabled > 0)
        return;

    tabEnabled = 1;

    document.getElementById('textareapost').addEventListener('keydown', function(event) {
        if (event.key == 'Tab')
        {
            event.preventDefault();
            addToArticle("\t", 0);
        }
    });
}