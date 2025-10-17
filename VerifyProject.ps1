# 快速测试脚本 - 验证 TulipAlg WPF 项目代码
# 此脚本用于检查代码完整性，不实际运行程序

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "TulipAlg 项目代码验证" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = "c:\Users\baochun\source\repos\TulipAlg\TulipAlg"

# 检查关键文件是否存在
$criticalFiles = @(
    "$projectRoot\Services\INavigationService.cs",
    "$projectRoot\Services\NavigationService.cs",
    "$projectRoot\Models\MenuItem.cs",
    "$projectRoot\ViewModels\MainWindowViewModel.cs",
    "$projectRoot\ViewModels\PointToPointViewModel.cs",
    "$projectRoot\ViewModels\LineToLineViewModel.cs",
    "$projectRoot\ViewModels\PointToLineViewModel.cs",
    "$projectRoot\ViewModels\FitCircleViewModel.cs",
    "$projectRoot\ViewModels\PointToCircleViewModel.cs",
    "$projectRoot\ViewModels\CircleToCircleViewModel.cs",
    "$projectRoot\ViewModels\PointToRegionViewModel.cs",
    "$projectRoot\Views\PointToPointView.xaml",
    "$projectRoot\Views\LineToLineView.xaml",
    "$projectRoot\Views\PointToLineView.xaml",
    "$projectRoot\Views\FitCircleView.xaml",
    "$projectRoot\Views\PointToCircleView.xaml",
    "$projectRoot\Views\CircleToCircleView.xaml",
    "$projectRoot\Views\PointToRegionView.xaml",
    "$projectRoot\MainWindow.xaml",
    "$projectRoot\App.xaml"
)

Write-Host "检查关键文件..." -ForegroundColor Yellow
$missingFiles = @()
foreach ($file in $criticalFiles) {
    if (Test-Path $file) {
        Write-Host "[✓] $($file.Replace($projectRoot + '\', ''))" -ForegroundColor Green
    } else {
        Write-Host "[✗] $($file.Replace($projectRoot + '\', ''))" -ForegroundColor Red
        $missingFiles += $file
    }
}

Write-Host ""
if ($missingFiles.Count -eq 0) {
    Write-Host "✓ 所有关键文件已创建！" -ForegroundColor Green
} else {
    Write-Host "✗ 缺少 $($missingFiles.Count) 个文件" -ForegroundColor Red
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "项目结构统计" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

$viewModels = Get-ChildItem -Path "$projectRoot\ViewModels" -Filter "*.cs" -ErrorAction SilentlyContinue
$views = Get-ChildItem -Path "$projectRoot\Views" -Filter "*.xaml" -ErrorAction SilentlyContinue
$services = Get-ChildItem -Path "$projectRoot\Services" -Filter "*.cs" -ErrorAction SilentlyContinue

Write-Host "ViewModels: $($viewModels.Count) 个" -ForegroundColor Cyan
Write-Host "Views: $($views.Count) 个" -ForegroundColor Cyan
Write-Host "Services: $($services.Count) 个" -ForegroundColor Cyan

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "构建建议" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "由于项目包含 C++ 依赖，请在 Visual Studio 2022 中：" -ForegroundColor Yellow
Write-Host "1. 打开 TulipAlg.slnx" -ForegroundColor White
Write-Host "2. 右键解决方案 -> 重定目标解决方案" -ForegroundColor White
Write-Host "3. 选择已安装的平台工具集（v143）" -ForegroundColor White
Write-Host "4. 构建并运行" -ForegroundColor White
Write-Host ""
Write-Host "或者在 PowerShell 中移除 C++ 项目引用后运行：" -ForegroundColor Yellow
Write-Host "dotnet run --project TulipAlg/TulipAlg.csproj" -ForegroundColor White
Write-Host ""
Write-Host "验证完成！" -ForegroundColor Green
