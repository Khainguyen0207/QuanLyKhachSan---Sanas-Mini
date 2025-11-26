$(document).ready(function () {
    $('input.tagify-input-fogdev').each(function () {
        new Tagify(this)
    });

    $('textarea.tagify-textarea-fogdev').each(function () {
        new Tagify(this)
    });
});