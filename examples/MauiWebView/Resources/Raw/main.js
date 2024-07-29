let activateButton = document.getElementById("activate-button");
let loadingIndicator = document.getElementById("loading-indicator");
let container = document.getElementById("container");
const port = window.location.protocol === 'https:' ? 18623 : 18622;
const host = window.location.protocol + '//' + "127.0.0.1:" + port;
var data = [];
let selectedThumbnail = null;

let queryDevicesButton = document.getElementById("query-devices-button");
queryDevicesButton.onclick = async () => {
    let scannerType = ScannerType.TWAINSCANNER | ScannerType.TWAINX64SCANNER;
    let devices = await getDevices(host, scannerType);
    let select = document.getElementById("sources");
    select.innerHTML = '';
    for (let i = 0; i < devices.length; i++) {
        let device = devices[i];
        let option = document.createElement("option");
        option.text = device['name'];
        option.value = JSON.stringify(device);
        select.add(option);
    };
}

let scanButton = document.getElementById("scan-button");
scanButton.onclick = async () => {
    let select = document.getElementById("sources");
    let device = select.value;

    if (device == null || device.length == 0) {
        alert('Please select a scanner.');
        return;
    }

    let inputText = document.getElementById("inputText").value;
    let license = inputText.trim();

    if (license == null || license.length == 0) {
        alert('Please input a valid license key.');
    }

    let parameters = {
        license: license,
        device: JSON.parse(device)['device'],
    };

    let showUICheck = document.getElementById("showUICheckId");

    let pixelTypeSelect = document.getElementById("pixelTypeSelectId");

    let resolutionSelect = document.getElementById("resolutionSelectId");

    let adfCheck = document.getElementById("adfCheckId");

    let duplexCheck = document.getElementById("duplexCheckId");

    parameters.config = {
        IfShowUI: showUICheck.checked,
        PixelType: pixelTypeSelect.selectedIndex,
        Resolution: parseInt(resolutionSelect.value),
        IfFeederEnabled: adfCheck.checked,
        IfDuplexEnabled: duplexCheck.checked,
    };


    let jobId = await scanDocument(host, parameters);
    let images = await getImages(host, jobId);

    for (let i = 0; i < images.length; i++) {
        let url = images[i];

        let img = document.getElementById('document-image');
        img.src = url;
        data.unshift(url);

        let option = document.createElement("option");
        option.selected = true;
        option.text = url;
        option.value = url;

        let thumbnails = document.getElementById("thumb-box");
        let newImage = document.createElement('img');
        newImage.setAttribute('src', url);
        if (thumbnails != null) {
            //thumbnails.appendChild(newImage);
            thumbnails.insertBefore(newImage, thumbnails.firstChild);
            newImage.addEventListener('click', e => {
                if (e != null && e.target != null) {
                    let target = e.target;
                    img.src = target.src;
                    selectedThumbnail = target;
                }
            });
        }

        selectedThumbnail = newImage;
    }

}

function rotateImage (imageId, angle) {
    const image = document.getElementById(imageId);
    const canvas = document.createElement('canvas');
    const context = canvas.getContext('2d');
    const imageWidth = image.naturalWidth;
    const imageHeight = image.naturalHeight;

    // Calculate the new rotation
    let rotation = 0;
    rotation = (rotation + angle) % 360;

    // Adjust canvas size for rotation
    if (rotation === 90 || rotation === -270 || rotation === 270) {
        canvas.width = imageHeight;
        canvas.height = imageWidth;
    } else if (rotation === 180 || rotation === -180) {
        canvas.width = imageWidth;
        canvas.height = imageHeight;
    } else if (rotation === 270 || rotation === -90) {
        canvas.width = imageHeight;
        canvas.height = imageWidth;
    } else {
        canvas.width = imageWidth;
        canvas.height = imageHeight;
    }

    // Clear the canvas
    context.clearRect(0, 0, canvas.width, canvas.height);

    // Draw the rotated image on the canvas
    context.save();
    if (rotation === 90 || rotation === -270) {
        context.translate(canvas.width, 0);
        context.rotate(90 * Math.PI / 180);
    } else if (rotation === 180 || rotation === -180) {
        context.translate(canvas.width, canvas.height);
        context.rotate(180 * Math.PI / 180);
    } else if (rotation === 270 || rotation === -90) {
        context.translate(0, canvas.height);
        context.rotate(270 * Math.PI / 180);
    }
    context.drawImage(image, 0, 0);
    context.restore();

    return canvas.toDataURL();
}

function getBase64Image() {
    var img = document.getElementById('document-image');
    var canvas = document.createElement('canvas');
    canvas.width = img.naturalWidth;
    canvas.height = img.naturalHeight;
    var ctx = canvas.getContext('2d');
    ctx.drawImage(img, 0, 0);

    var dataURL = canvas.toDataURL('image/png'); 
    var base64 = dataURL.split(',')[1]; 
    return base64;
}

let saveButton = document.getElementById("save-button");
saveButton.onclick = async () => {
    window.location.href = 'invoke://CallCSharpFunction';    
}

// Delete all images
let deleteButton = document.getElementById("delete-button");
deleteButton.onclick = async () => {
    let img = document.getElementById('document-image');
    img.src = 'images/default.png';
    data = [];
    let thumbnails = document.getElementById("thumb-box");
    thumbnails.innerHTML = '';
}

// Rotate image
let rotateLeftButton = document.getElementById("rotate-left-button");
rotateLeftButton.onclick = () => {
    let img = document.getElementById('document-image');
    img.src = rotateImage('document-image', -90);
    selectedThumbnail.src = img.src;
}

let rotateRightButton = document.getElementById("rotate-right-button");
rotateRightButton.onclick = () => {
    let img = document.getElementById('document-image');
    img.src = rotateImage('document-image', 90);
    selectedThumbnail.src = img.src;
}

const ScannerType = {
    // TWAIN scanner type, represented by the value 0x10
    TWAINSCANNER: 0x10,

    // WIA scanner type, represented by the value 0x20
    WIASCANNER: 0x20,

    // 64-bit TWAIN scanner type, represented by the value 0x40
    TWAINX64SCANNER: 0x40,

    // ICA scanner type, represented by the value 0x80
    ICASCANNER: 0x80,

    // SANE scanner type, represented by the value 0x100
    SANESCANNER: 0x100,

    // eSCL scanner type, represented by the value 0x200
    ESCLSCANNER: 0x200,

    // WiFi Direct scanner type, represented by the value 0x400
    WIFIDIRECTSCANNER: 0x400,

    // WIA-TWAIN scanner type, represented by the value 0x800
    WIATWAINSCANNER: 0x800
};

// Get available scanners
async function getDevices(host, scannerType) {
    devices = [];
    // Device type: https://www.dynamsoft.com/web-twain/docs/info/api/Dynamsoft_Enum.html
    // http://local.dynamsoft.com:18622/DWTAPI/Scanners?type=64 for TWAIN only
    let url = host + '/DWTAPI/Scanners'
    if (scannerType != null) {
        url += '?type=' + scannerType;
    }

    try {
        let response = await fetch(url);

        if (response.ok) {
            let devices = await response.json();
            return devices;
        }
    } catch (error) {
        console.log(error);
    }
    return [];
}

// Create a scan job by feeding one or multiple physical documents
async function scanDocument(host, parameters, timeout = 30) {
    let url = host + '/DWTAPI/ScanJobs?timeout=' + timeout;

    try {
        let response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(parameters)
        });

        if (response.ok) {
            let jobId = await response.text();
            return jobId;
        }
        else {
            return '';
        }
    } catch (error) {
        alert(error);
        return '';
    }
}

// Delete a scan job by job id
async function deleteJob(host, jobId) {
    if (!jobId) return;

    let url = host + '/DWTAPI/ScanJobs/' + jobId;
    console.log('Delete job: ' + url);
    axios({
        method: 'DELETE',
        url: url
    })
        .then(response => {
            // console.log('Status:', response.status);
        })
        .catch(error => {
            // console.log('Error:', error);
        });
}

// Get document image streams by job id
async function getImages(host, jobId) {
    let images = [];
    let url = host + '/DWTAPI/ScanJobs/' + jobId + '/NextDocument';

    while (true) {
        try {

            let response = await fetch(url);

            if (response.status == 200) {
                const arrayBuffer = await response.arrayBuffer();
                const blob = new Blob([arrayBuffer], { type: response.type });
                const imageUrl = URL.createObjectURL(blob);

                images.push(imageUrl);
            }
            else {
                break;
            }

        } catch (error) {
            console.error('No more images.');
            break;
        }
    }

    return images;
}
