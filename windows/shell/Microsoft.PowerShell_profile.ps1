# FriendlyTerminal shell integration for PowerShell.
# Emits OSC 133 markers so the app can split output into per-command blocks
# and read the working directory. See docs/behavior-spec/shell-integration.md.

$global:__ftEsc = [char]27

function global:__ftOsc($body) {
    [Console]::Write("$global:__ftEsc]$body$global:__ftEsc\")
}

$global:__ftOriginalPrompt = $function:prompt
$global:__ftRan = $false

function global:prompt {
    $code = $LASTEXITCODE
    if ($global:__ftRan) { __ftOsc "133;D;$code" }
    __ftOsc "133;A"
    __ftOsc "9;9;$((Get-Location).Path)"
    $text = & $global:__ftOriginalPrompt
    __ftOsc "133;B"
    $global:__ftRan = $false
    return $text
}

if (Get-Module -ListAvailable PSReadLine) {
    Set-PSReadLineKeyHandler -Key Enter -ScriptBlock {
        __ftOsc "133;C"
        $global:__ftRan = $true
        [Microsoft.PowerShell.PSConsoleReadLine]::AcceptLine()
    }
}
