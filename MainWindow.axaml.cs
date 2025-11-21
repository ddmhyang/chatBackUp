using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage; // 파일 열기용
using MiniExcelLibs;
using MsBox.Avalonia; // 메시지 박스용
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using VisualBackUpApp;

namespace VisualBackUpApp
{
    public partial class MainWindow : Window
    {
        private const string DEFAULT_BG = "https://images.unsplash.com/photo-1637325258040-d2f09636ecf6?q=80&w=870&auto=format&fit=crop";

        // HTML 템플릿 (WPF 때 썼던 거 그대로 넣으세요!)
        private const string HtmlTemplate = """
        <!DOCTYPE html>
        <html lang="ko">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <style>
                .container {
                    width: 90%;
                    max-width: 1280px;
                    aspect-ratio: 16 / 9;
                    position: relative;
                    overflow: hidden;
                    background-repeat: no-repeat;
                    background-position: center center;
                    background-size: cover;
                    box-shadow: 0 0 20px rgba(0,0,0,0.5);
                    container-type: inline-size;
                    cursor: pointer;
                    transition: background-image 0.5s ease-in-out;
                }

                .character_layer {
                    position: absolute;
                    bottom: 0 !important;
                    left: 0 !important;
                    width: 100% !important;
                    height: 100% !important;
                    z-index: 1;
                    pointer-events: none;
                    display: flex !important;
                    align-items: flex-end !important;
                }

                .pos_left { 
                    justify-content: flex-start !important;
                    transform: none !important;
                }

                .pos_center {
                    justify-content: center !important;
                    transform: none !important;
                    left: 0 !important;
                }

                .pos_right { 
                    justify-content: flex-end !important;
                    transform: none !important;
                }

                .character_img {
                    height: 100% !important;
                    width: auto !important;
                    object-fit: contain;
                    max-width: unset !important;
                    margin: 0 !important;
                    padding: 0 !important;
                    border: 0 !important;
                    display: block !important;
                    filter: drop-shadow(5px 5px 5px rgba(0,0,0,0.5));
                }

                .ui_layer {
                    aspect-ratio: 16 / 3.8;
                    position: absolute;
                    bottom: 3%;
                    left: 50%;
                    transform: translateX(-50%);
                    width: 95%;
                    z-index: 2;
                    display: flex;
                    flex-direction: column;
                }

                .name_box {
                    background-color: rgba(0, 0, 0, 0.8);
                    color: #fff;
                    padding: 1.1cqw 4.5cqw;
                    border-radius: 10px 10px 0 0;
                    width: fit-content;
                    font-weight: bold;
                    font-size: 1.8cqw;
                    margin-bottom: 0;
                    border: 2px solid #fff;
                    border-bottom: none;
                }

                .txt_box {
                    flex: 1;
                    background-color: rgba(0, 0, 0, 0.7);
                    border: 2px solid #fff;
                    border-radius: 10px;
                    border-top-left-radius: 0;
                    padding: 2cqw;
                    box-sizing: border-box;
                    color: white;
                    font-size: 2cqw;
                    line-height: 3cqw;
                }
            </style>
        </head>
        <body>

            <div class="container">
                <div class="character_layer pos_left">
                    <img class="character_img" src="">
                </div>

                <div class="ui_layer">
                    <div class="name_box">이름</div> 
                    <div class="txt_box">내용</div>
                </div>
            </div>

        <script>
            const scenario = {{DATA_INSERT_HERE}};

            const container = document.querySelector('.container');
            const nameBox = document.querySelector('.name_box');
            const txtBox = document.querySelector('.txt_box');
            const charImg = document.querySelector('.character_img');
            const charLayer = document.querySelector('.character_layer');

            let currentIndex = 0;

            function updateScene() {
                const currentData = scenario[currentIndex];

                if (!currentData) {
                    currentIndex = 0;
                    updateScene();
                    return;
                }

                nameBox.textContent = currentData.name;
                txtBox.innerHTML = currentData.text;

                if (currentData.img) {
                    charImg.style.display = 'block';
                    charImg.src = currentData.img;
                } else {
                    charImg.style.display = 'none';
                }

                if (!currentData.name || currentData.name === "nan") {
                    nameBox.style.display = 'none';
                } else {
                    nameBox.style.display = 'block'; 
                }

                charLayer.classList.remove('pos_left', 'pos_right', 'pos_center');

                if (currentData.position === 'right') {
                    charLayer.classList.add('pos_right');
                } else if (currentData.position === 'center') {
                    charLayer.classList.add('pos_center');
                } else {
                    charLayer.classList.add('pos_left');
                }

                if (currentData.bg) {
                    container.style.backgroundImage = `url("${currentData.bg}")`;
                }
            }

            container.addEventListener('click', function() {
                currentIndex++;
                updateScene(); 
            });

            updateScene();

        </script>
        </body>
        </html>
        """;

        public MainWindow()
        {
            InitializeComponent();
        }

        // [1. 엑셀 로드] - 비동기(async)로 바뀜!
        private async void BtnLoadExcel_Click(object sender, RoutedEventArgs e)
        {
            // WPF의 OpenFileDialog 대신 StorageProvider 사용
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "엑셀 파일 선택",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.All, FilePickerFileTypes.TextPlain }
                // 맥은 확장자 필터가 까다로워서 일단 All로 두는 게 편합니다.
            });

            if (files.Count >= 1)
            {
                // 맥/윈도우 경로 차이를 알아서 처리해주는 Path
                GameDataManager.ExcelFilePath = files[0].Path.LocalPath;
                AnalyzeExcel(GameDataManager.ExcelFilePath);

                BtnFace.IsEnabled = true;
                BtnBg.IsEnabled = true;
                BtnGen.IsEnabled = true;
            }
        }

        private void AnalyzeExcel(string path)
        {
            try
            {
                var rows = MiniExcel.Query(path);
                var usedFaceIds = new HashSet<int>();
                var usedBgIds = new HashSet<int>();

                foreach (var row in rows)
                {
                    var data = (IDictionary<string, object>)row;
                    // 스마트 값 찾기 (헬퍼 함수 사용)
                    string faceVal = GetSmartValue(data, "face", "C");
                    if (int.TryParse(faceVal, out int fid)) usedFaceIds.Add(fid);

                    string bgVal = GetSmartValue(data, "background", "E");
                    if (int.TryParse(bgVal, out int bid)) usedBgIds.Add(bid);
                }

                GameDataManager.Faces.Clear();
                GameDataManager.Backgrounds.Clear();

                foreach (int id in usedFaceIds)
                    GameDataManager.Faces.Add(new ResourceItem { Id = id, Name = $"Face {id}", Url = "" });

                foreach (int id in usedBgIds)
                    GameDataManager.Backgrounds.Add(new ResourceItem { Id = id, Name = $"Background {id}", Url = "" });

                TxtStatus.Text = $"분석 완료 (Face: {usedFaceIds.Count}개)";
            }
            catch (Exception ex)
            {
                // MsBox 사용법
                MessageBoxManager.GetMessageBoxStandard("오류", "읽기 실패: " + ex.Message).ShowAsync();
            }
        }

        // [2. 설정 버튼들] - 비동기(async)로 띄워야 함
        private async void BtnFace_Click(object sender, RoutedEventArgs e)
        {
            var win = new ManagerWindow("Face 설정", GameDataManager.Faces);
            await win.ShowDialog(this); // await 필수
        }

        private async void BtnBg_Click(object sender, RoutedEventArgs e)
        {
            var win = new ManagerWindow("배경 설정", GameDataManager.Backgrounds);
            await win.ShowDialog(this);
        }

        // [3. 생성 버튼]
        private async void BtnGen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rows = MiniExcel.Query(GameDataManager.ExcelFilePath);
                var scenarioList = new List<object>();

                foreach (var row in rows)
                {
                    var data = (IDictionary<string, object>)row;

                    // 데이터 읽기 & 빈 줄 제거 로직 (WPF 최신 버전 복붙하세요)
                    string name = GetSmartValue(data, "name", "A");
                    string msg = GetSmartValue(data, "message", "B");
                    string faceVal = GetSmartValue(data, "face", "C");
                    string bgVal = GetSmartValue(data, "background", "E");
                    string pos = GetSmartValue(data, "position", "D").ToLower();

                    if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(msg) &&
                        string.IsNullOrWhiteSpace(faceVal) && string.IsNullOrWhiteSpace(bgVal)) continue;

                    if (name == "name" && msg == "message") continue;
                    if (string.IsNullOrEmpty(pos)) pos = "center";

                    // 이미지 매핑 로직 (WPF와 동일)
                    string imgUrl = "";
                    if (int.TryParse(faceVal, out int fid))
                    {
                        var item = GameDataManager.Faces.FirstOrDefault(c => c.Id == fid);
                        if (item != null) imgUrl = item.Url;
                    }

                    string bgUrl = DEFAULT_BG;
                    if (int.TryParse(bgVal, out int bid))
                    {
                        var item = GameDataManager.Backgrounds.FirstOrDefault(b => b.Id == bid);
                        if (item != null && !string.IsNullOrEmpty(item.Url)) bgUrl = item.Url;
                    }

                    scenarioList.Add(new { name = name, text = msg, img = imgUrl, position = pos, bg = bgUrl });
                }

                var jsonOptions = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                string jsonString = JsonSerializer.Serialize(scenarioList, jsonOptions);
                string finalHtml = HtmlTemplate.Replace("{{DATA_INSERT_HERE}}", jsonString);

                // ★ 저장하기 (StorageProvider 사용)
                var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "HTML 파일 저장",
                    DefaultExtension = "html",
                    SuggestedFileName = "visual_novel.html",
                    FileTypeChoices = new[] { new FilePickerFileType("HTML Files") { Patterns = new[] { "*.html" } } }
                });

                if (file != null)
                {
                    // 스트림으로 저장해야 맥/윈도우 호환성 좋음
                    await using var stream = await file.OpenWriteAsync();
                    using var writer = new StreamWriter(stream);
                    await writer.WriteAsync(finalHtml);

                    await MessageBoxManager.GetMessageBoxStandard("성공", "저장되었습니다!").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard("오류", ex.Message).ShowAsync();
            }
        }

        // 헬퍼 함수 (WPF랑 동일)
        private string GetSmartValue(IDictionary<string, object> row, string keyName, string colLetter)
        {
            foreach (var key in row.Keys) if (string.Equals(key.Trim(), keyName, StringComparison.OrdinalIgnoreCase)) return row[key]?.ToString() ?? "";
            foreach (var key in row.Keys) if (string.Equals(key.Trim(), colLetter, StringComparison.OrdinalIgnoreCase)) return row[key]?.ToString() ?? "";
            return "";
        }
    }
}