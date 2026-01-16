mergeInto(LibraryManager.library, {
    OpenFilePicker: function() {
        var input = document.createElement('input');
        input.type = 'file';
        input.accept = '.csv';
        
        input.onchange = function(event) {
            var file = event.target.files[0];
            if (file) {
                var reader = new FileReader();
                reader.onload = function(e) {
                    var content = e.target.result;
                    // Send to Unity
                    SendMessage('WebFileUpload', 'OnFileSelected', content);
                };
                reader.readAsText(file);
            }
        };
        
        input.click();
    }
});
