function sharePhoto() {
    var uploader = document.getElementById('snap').files;
    if (uploader.length > 0) {
        document.getElementById("loader").style.display = "block";

        var formData = new FormData();
        formData.append("opmlFile", uploader[0]);

        var request = new XMLHttpRequest();
        request.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                document.getElementById('analyticsResult').innerText = request.responseText;
                window.location.hash = '#flcontent';
                document.getElementById("loader").style.display = "none";
            }
        };
        request.open("post", "/api/Image", true);
        request.send(formData);
    }
    else
        alert("Select an image");

}

function message()
{
    document.getElementById("loaderfl").style.display = "block";
    var message = document.getElementById("feeling").value;
    var request = new XMLHttpRequest();
    request.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            document.getElementById('sentimentResult').innerText = request.responseText;
            window.location.hash = '#feeling';
            document.getElementById("loaderfl").style.display = "none";
        }
    };
    request.open("get", "/api/Image?message=" +message, true);
    request.send();    
}
