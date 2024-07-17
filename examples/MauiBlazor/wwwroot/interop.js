var data = [];


window.jsFunctions = {
    getDevices: async function (host, scannerType, selectId) {
        let select = document.getElementById(selectId);
        select.innerHTML = '';
        try {
            let url = host + '/DWTAPI/Scanners';
            if (scannerType != null || scannerType !== '') {
                url += '?type=' + scannerType;
            }
            
            let response = await fetch(url);

            if (response.ok) {
                let devices = await response.json();

                for (let i = 0; i < devices.length; i++) {
                    let device = devices[i];
                    let option = document.createElement("option");
                    option.text = device['name'];
                    option.value = JSON.stringify(device);
                    select.add(option);
                };

                return devices;
            }
            else {
                return "";
            }
        } catch (error) {
            alert(error);
            return "";
        }
    },
    scanDocument: async function (host, licenseKey, sourceSelectId, pixelTypeSelectId, resolutionSelectId, showUICheckId, adfCheckId, duplexCheckId, timeout = 30) {
        let select = document.getElementById(sourceSelectId);
        let scanner = select.value;

        if (scanner == null || scanner.length == 0) {
            alert('Please select a scanner.');
            return;
        }

        if (licenseKey == null || licenseKey.length == 0) {
            alert('Please input a valid license key.');
        }

        let showUICheck = document.getElementById(showUICheckId);

        let pixelTypeSelect = document.getElementById(pixelTypeSelectId);

        let resolutionSelect = document.getElementById(resolutionSelectId);

        let adfCheck = document.getElementById(adfCheckId);

        let duplexCheck = document.getElementById(duplexCheckId);

        let parameters = {
            license: licenseKey,
            device: JSON.parse(scanner)['device'],
        };

        parameters.config = {
            IfShowUI: showUICheck.checked,
            PixelType: pixelTypeSelect.selectedIndex,
            Resolution: parseInt(resolutionSelect.value),
            IfFeederEnabled: adfCheck.checked, 
            IfDuplexEnabled: duplexCheck.checked,
        };
        

        // REST endpoint to create a scan job
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
                let images = await getImages(host, jobId, 'images');
                return images;
            }
            else {
                return [];
            }
        } catch (error) {
            alert(error);
            return [];
        }
    
    },

    fetchImageAsBase64: async function (url) {
        try {
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            const arrayBuffer = await response.arrayBuffer();
            const blob = new Blob([arrayBuffer], { type: response.type });
            const reader = new FileReader();

            return new Promise((resolve, reject) => {
                reader.onloadend = () => resolve(reader.result.split(',')[1]);
                reader.onerror = reject;
                reader.readAsDataURL(blob);
            });
        } catch (error) {
            console.error('Error fetching image:', error);
            return null;
        }
    },

    displayAlert: function(message) {
        alert(message);
    },

    rotateImage: async function (imageId, angle) {
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
};

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

async function deleteJob(host, jobId) {
    let url = host + "/DWTAPI/ScanJobs/" + jobId;
    const response = await fetch(url, { "method": "DELETE", "mode": "cors", "credentials": "include" });
    if (response.status == 200) {
        console.log(`job ${jobId} deleted.`);
    }
}
