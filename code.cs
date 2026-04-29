/**
 * =================================================================
 * BACKEND API - ATHAYA TAHFIDZ & ATTENDANCE SYSTEM (FINAL)
 * Database: Google Sheets
 * Developer: oosho studio
 * =================================================================
 */

// 1. SETUP DATABASE (Jalankan satu kali dari menu toolbar)
function setupDatabase() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var dbSchema = {
    'Santri': ['id', 'name', 'nis', 'kelas', 'halaqoh', 'targetJuz', 'pencapaianJuz', 'totalHalaman', 'status', 'password', 'waliPassword'],
    'Guru': ['id', 'name', 'halaqoh', 'totalSantri', 'totalSetoran', 'username', 'password'],
    'Kegiatan': ['id', 'date', 'santriId', 'type', 'surah', 'ayat', 'halaman', 'nilai', 'status', 'jenisMurojaah', 'kelancaran', 'guru', 'catatan'],
    'Absensi': ['id', 'date', 'santriId', 'sesi', 'status', 'guru', 'catatan'],
    'Ujian': ['id', 'date', 'santriId', 'type', 'targetUjian', 'nilai', 'status', 'penguji', 'catatan'],
    'Halaqoh': ['id', 'name'],
    'Alumni': ['id', 'name', 'nis', 'tahunLulus', 'pencapaianJuz', 'predikat'],
    'Evaluasi': ['id', 'date', 'santriId', 'periode', 'kelancaran', 'tajwid', 'adab', 'catatan']
  };

  for (var sheetName in dbSchema) {
    var sheet = ss.getSheetByName(sheetName);
    if (!sheet) {
      sheet = ss.insertSheet(sheetName);
      sheet.appendRow(dbSchema[sheetName]);
      sheet.getRange(1, 1, 1, dbSchema[sheetName].length).setFontWeight("bold").setBackground("#059669").setFontColor("white");
      sheet.setFrozenRows(1);
    }
  }
  return "Sistem Siap! Database Athaya telah dibuat.";
}

// 2. ENTRY POINT GET (Ambil Data)
function doGet(e) {
  var data = fetchAllData();
  return ContentService.createTextOutput(JSON.stringify(data)).setMimeType(ContentService.MimeType.JSON);
}

// 3. ENTRY POINT POST (Simpan/Update Data)
function doPost(e) {
  try {
    var params = JSON.parse(e.postData.contents);
    var result = handleSync(params.action, params.sheetName, params.data);
    return ContentService.createTextOutput(JSON.stringify(result)).setMimeType(ContentService.MimeType.JSON);
  } catch (err) {
    return ContentService.createTextOutput(JSON.stringify({success: false, error: err.toString()})).setMimeType(ContentService.MimeType.JSON);
  }
}

function fetchAllData() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var result = {};
  var sheets = ['Santri', 'Guru', 'Kegiatan', 'Absensi', 'Ujian', 'Halaqoh', 'Alumni', 'Evaluasi'];
  
  sheets.forEach(function(sName) {
    var sheet = ss.getSheetByName(sName);
    if (!sheet) return;
    var values = sheet.getDataRange().getValues();
    if (values.length <= 1) { result[sName] = []; return; }
    
    var headers = values[0];
    var rows = [];
    for (var i = 1; i < values.length; i++) {
      var obj = {};
      for (var j = 0; j < headers.length; j++) {
        var val = values[i][j];
        if (val instanceof Date) val = Utilities.formatDate(val, Session.getScriptTimeZone(), "yyyy-MM-dd");
        obj[headers[j]] = val;
      }
      rows.push(obj);
    }
    result[sName] = rows;
  });
  return result;
}

function handleSync(action, sheetName, dataObj) {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getSheetByName(sheetName);
  if (!sheet) return { success: false };

  if (action === 'insert') {
    var headers = sheet.getRange(1, 1, 1, sheet.getLastColumn()).getValues()[0];
    var newRow = headers.map(function(h) { return dataObj[h] !== undefined ? dataObj[h] : ''; });
    sheet.appendRow(newRow);
  } 
  else if (action === 'update' || action === 'delete') {
    var values = sheet.getDataRange().getValues();
    var idIndex = values[0].indexOf('id');
    for (var i = 1; i < values.length; i++) {
      if (values[i][idIndex] == dataObj.id) {
        if (action === 'update') {
          var updatedRow = values[0].map(function(h) {
            return dataObj[h] !== undefined ? dataObj[h] : values[i][values[0].indexOf(h)];
          });
          sheet.getRange(i + 1, 1, 1, values[0].length).setValues([updatedRow]);
        } else {
          sheet.deleteRow(i + 1);
        }
        break;
      }
    }
  }
  return { success: true };
}
