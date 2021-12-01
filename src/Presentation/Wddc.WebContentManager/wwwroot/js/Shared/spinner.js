/**
 * Spinner object for spinning animation when updating/posting
 * changes made on purchasing forms
 */

/**
 * Initializes spinner object with the pre-set options
 * @param {string} targetId The element id of the spinner
 * @returns {Spinner} The created spinner object
 */
function startSpinner(targetId) {
    var options = {
        lines: 15, // The number of lines to draw
        length: 0, // The length of each line
        width: 10, // The line thickness
        radius: 20, // The radius of the inner circle
        scale: 1, // Scales overall size of the spinner
        corners: 1, // Corner roundness (0..1)
        color: '#D0D0D0',
        fadeColor: 'transparent', // CSS color or array of colors
        speed: 1, // Rounds per second
        rotate: 0, // The rotation offset
        animation: 'spinner-line-fade-more', // The CSS animation name for the lines
        direction: 1, // 1: clockwise, -1: counterclockwise
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        className: 'spinner', // The CSS class to assign to the spinner
        top: '50%', // Top position relative to parent
        left: '50%', // Left position relative to parent
        shadow: '0 0 1px transparent', // Box-shadow for the lines
        position: 'absolute' // Element positioning
    };

    var target = document.getElementById(targetId);
    var spinner = new Spinner(options).spin(target);
    return spinner;
}

/**
 * Stops spinner animation
 * @param {Spinner} spinner The spinner element
 */
function stopSpinner(spinner) {
    spinner.stop();
}