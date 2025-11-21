using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;

namespace VisualBackUpApp
{
    public partial class ManagerWindow : Window
    {
        // 생성자 1: 데이터 받아서 열릴 때
        public ManagerWindow(string title, List<ResourceItem> list)
        {
            InitializeComponent();

            // XAML에 있는 컨트롤 이름(TxtTitle, DgList)을 찾아서 설정
            var txtTitle = this.FindControl<TextBlock>("TxtTitle");
            var dgList = this.FindControl<DataGrid>("DgList");

            if (txtTitle != null) txtTitle.Text = title;
            if (dgList != null) dgList.ItemsSource = list;
        }

        // 생성자 2: 디자이너(미리보기)용 - 지우면 안 됨
        public ManagerWindow()
        {
            InitializeComponent();
        }

        // 저장 버튼 클릭
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Close(); // 창 닫기
        }
    }
}
// ▲ 이 맨 마지막 괄호가 없어서 생긴 오류였습니다!