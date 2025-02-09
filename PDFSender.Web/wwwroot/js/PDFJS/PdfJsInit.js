/* PDFJS integration script */

var url;/* = 'E:/Temp/PDF Sender Folder/PDF2.pdf';*/
var changescale = 5;
function ShowOverlay()
{
    document.getElementById("MainContainer").style.display = "block";
    document.querySelector("body").style.overflow = "hidden";   
}

function HideOverlay()
{
    document.getElementById("MainContainer").style.display = "none";
    document.querySelector("body").style.overflow = "auto";
}

function RenderPDF(blobData)
{
    //url= strUrl;

    var canvasContainer = document.getElementById('canvasContainer');
    var pdfDocument;

    const blob = new Blob([blobData], { type: 'application/pdf' });
    const url = URL.createObjectURL(blob);

    pdfjsLib.getDocument(url).promise.then(RenderPages);
    function RenderPages(pdfDoc)
    {
        pdfDocument = pdfDoc;
        document.getElementById("AllPages").innerHTML = pdfDoc.numPages;
        for(var num = 1; num <= pdfDoc.numPages; num++)
            pdfDoc.getPage(num).then(RenderPage);
    }
    function RenderPage(page)
    {
        var viewport = page.getViewport({ scale: changescale });
        var wrapper = document.createElement("div");
        wrapper.className = "canvas-wrapper";
        wrapper.id = `canvas-wrapper${page._pageIndex + 1}`;
        var canvas = document.createElement('canvas');
        var ctx = canvas.getContext('2d');
        var renderContext = { canvasContext: ctx, viewport: viewport };
        canvas.height = viewport.height;
        canvas.width = viewport.width;
        canvas.className = "canvas";
        canvas.style.width = "100%";
        wrapper.appendChild(canvas)
        canvasContainer.appendChild(wrapper);
        page.render(renderContext);
    }
}

function ZoomIn(event)
{
    var width = parseInt(document.getElementById('canvasContainer').style.width) ? parseInt(document.getElementById('canvasContainer').style.width) : 70;
    if(width < 150)
    {
        width += 10;
        document.getElementById('canvasContainer').style.width = ` ${width}%`;
        document.getElementById('selectTag').options[0].innerHTML = ` ${width}%`;
    }
}

function ZoomOut(event)
{
    var width = parseInt(document.getElementById('canvasContainer').style.width) ? parseInt(document.getElementById('canvasContainer').style.width) : 70;
    if(width > 10)
    {
        width -= 10;
        document.getElementById('canvasContainer').style.width = ` ${width}%`;
        document.getElementById('selectTag').options[0].innerHTML = ` ${width}%`;
    }
}

function AddMouseEvent(e)
{
    PDFCurrentPageNumber();
    if(e.ctrlKey == true)
    {
        e.preventDefault();
        e.stopPropagation();
        if(e.deltaY > 0) ZoomIn();
        else ZoomOut();
    }
};

function PDFPageUp()
{
    var inc = parseInt(document.getElementById("PageNumber").value);
    if(inc < parseInt(document.getElementById("AllPages").innerHTML))
    {
        inc = inc + 1;
        document.getElementById("PageNumber").value = inc;
        PDFPageChange();
    }
}

function PDFPageDown()
{
    var inc = parseInt(document.getElementById("PageNumber").value);
    if(inc > 0)
    {
        inc = inc - 1;
        document.getElementById("PageNumber").value = inc;
        PDFPageChange();
    }
}

function PDFPageChange()
{
    var pageNumber = parseInt(document.getElementById("PageNumber").value);
    var target = `#canvas-wrapper${pageNumber}`;
    window.location.hash = target;
}

function PDFWidthChange()
{
    if(document.getElementById('canvasContainer').style.width !== "100%")
    {
        document.getElementById('canvasContainer').style.width = "100%";
        document.getElementById('ToggleIcon').className = "fa fa-square-o";
        document.getElementById('selectTag').options[0].innerHTML = "100%";
    }
    else
    {
        document.getElementById('canvasContainer').style.width = "75%";
        document.getElementById('ToggleIcon').className = "far fa-clone";
        document.getElementById('selectTag').options[0].innerHTML = "75%";
    }
}
function PDFWidthChangeUsingSelectTag()
{
    var selectedOption = parseInt(document.getElementById('selectTag').value);

    if(selectedOption == 0) document.getElementById('canvasContainer').style.width = "75%";
    else if(selectedOption == 1) document.getElementById('canvasContainer').style.width = "50%";
    else if(selectedOption == 2) document.getElementById('canvasContainer').style.width = "100%";
    else if(selectedOption == 3) document.getElementById('canvasContainer').style.width = "200%";
    else if(selectedOption == 4) document.getElementById('canvasContainer').style.width = "400%";
    else if(selectedOption == 5) document.getElementById('canvasContainer').style.width = "35%";
}

function PDFCurrentPageNumber()
{
    const overlay = document.getElementById("overlay");
    const TotalPages = document.querySelectorAll(".canvas-wrapper");
    document.getElementById("PageNumber").value = parseInt((overlay.scrollTop + (window.innerHeight / 2)) / (overlay.scrollHeight / TotalPages.length)) + 1;
}

function DownloadFile()
{
    const currentTime = new Date().toLocaleTimeString();
    var fileName = `PDF-${Date.now()} - ${currentTime}`;

    fetch(url, { method: 'get', mode: 'no-cors', referrerPolicy: 'no-referrer' }).then(res => res.blob()).then(res =>
    {
        const aElement = document.createElement('a');
        aElement.setAttribute('download', fileName);
        aElement.href = URL.createObjectURL(res);
        aElement.setAttribute('target', '_blank');
        aElement.click();
        URL.revokeObjectURL(href);
    });
};

function AppendConsoleError(strException)
{
    console.log(strException);
}