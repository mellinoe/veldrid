$fileNames = Get-ChildItem -Path $scriptPath -Recurse

foreach ($file in $fileNames)
{
    if ($file.Name.EndsWith("hlsl.vert") -Or $file.Name.EndsWith("hlsl.frag"))
    {
        Write-Host "Compiling $file"
        glslangvalidator -e main -V -D $file -o $file".spv"
    }
    elseif ($file.Name.EndsWith("vert") -Or $file.Name.EndsWith("frag") -Or $file.Name.EndsWith("comp"))
    {
        Write-Host "Compiling $file"
        glslangvalidator -V $file -o $file".spv"
    }
}