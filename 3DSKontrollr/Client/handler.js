// This file is part of 3DSKontrollr
// All this code is executed on the Nintendo 3DS
// Usually, you don't need to change anything in this file
// But if you want to tweak the way 3DSKontrollr works to adjust your needs, you are free to do anything you want.

var int;
var int2;
function SR(data)
{
    xmlhttp=new XMLHttpRequest();
    xmlhttp.open("GET",data,true);
    xmlhttp.send();
}

window.setInterval(function () {
    window.scrollTo(40, 220);
}, 50);

window.onload = function () {
    "use strict";
    var connectButton = document.getElementById('connection'),
        keyMap = {
            37: 'left',
            38: 'up',
            39: 'right',
            40: 'down',
            13: 'a'
        },
        photoupload = document.getElementById('photoupd'),
        fileupload = document.getElementById('fileinput'),
        bodee = document.body,
        currprc = document.getElementById('currproc'),
        currprcicn = document.getElementById('currprocicn'),
        currscn = document.getElementById('currsc'),
        btm = document.getElementById('toucharea'),
        dtb = false;


    btm.onmousemove = function (e) {
        dtb = true;
        var ofX = e.clientX - 40;
        var ofY = e.clientY - 252;
        if(ofY > 0)
        {
            e = e || window.event;
            var RelX = ofX / 320;
            var RelY = ofY / 180;
            SR('touch-' + RelX + '-' + RelY);
        }
    }


    function RefreshCurrProc()
    {
        var oldproc = currprc.innerHTML;
        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4) {
                if(oldproc != xhr.responseText)
                {
                    currprc.innerHTML = xhr.responseText;
                    currprcicn.src = '';
                    currprcicn.src = '/src/Client/CurrentWindowIcon.png?' + new Date().getTime();;
                }
            }
        }
        xhr.open('GET', 'src/client/CurrWindow.txt', true);
        xhr.send();
    }

    function RefreshCurrSc() {
                    currscn.src = '' ;
                    currscn.src = '/sc.jpg?' + new Date().getTime();
        }

    connectButton.addEventListener('click', function(ev) {
        ev.preventDefault();
        switch (connectButton.innerText) {
            case 'Conectar':
                alert("Esta app está en fase beta y es experimental\nSi encuentras un bug, notifícalo a TheBITLINK.");
                if (confirm("¿Quieres conectarte a \n" + document.domain + "?"))
                {
                    SR('connect');
                    connectButton.innerText = 'Desconectar';
                    //photoupload.style.display = 'block';
                    int = setInterval(RefreshCurrProc, 1100);
                    int2 = setInterval(RefreshCurrSc, 200);
                }
                break;
            case 'Desconectar':
                if (confirm("¿Quieres desconectarte de \n" + document.domain + "?")) {
                    SR('disconnect');
                    connectButton.innerText = 'Conectar';
                    //photoupload.style.display = 'none';
                    clearInterval(int);
                    //clearInterval(int2);
                    currprc.innerHTML = '';
                    currprcicn.src = '';
                }
                break;
        }
    });

    

    bodee.addEventListener('click', function (ev) {
        ev.preventDefault();
    });

    photoupload.addEventListener('click', function (ev) {
        fileupload.click();
    });

    fileupload.addEventListener('change', function (ev) {
        var xhr = new XMLHttpRequest();
        xhr.open("POST", "upload", true);
        var formData = new FormData();
        formData.append("file", this.files[0]);
        xhr.send(formData);
    });

    document.onkeydown = function(ev) {
        var which = ev.keyCode || ev.which,
            key = keyMap[which];

        if (key) {
            ev.preventDefault();
            SR(key + '.press');
        }
    };

    document.onkeyup = function(ev) {
        var which = ev.keyCode || ev.which,
            key = keyMap[which];

        if (key) {
            ev.preventDefault();
            SR(key+'.release');
        }
    };

    connectButton.addEventListener('blur', function (ev) {
        if (!dtb)
        {
            SR('b.press');
            setTimeout(function () { SR('b.release') }, 50);
            this.focus();
        }
        else {
            dtb = false;
        }
        
    });

    //window.addEventListener("resize", function (ev) {
    //    alert('X o Y, que wea' + window.innerWidth + ',' + window.innerHeight);
    //    ev.preventDefault();
    //});
};
