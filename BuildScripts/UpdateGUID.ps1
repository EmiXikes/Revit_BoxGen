if ($args.Length -eq 1)
{
	if (Test-Path $args[0])
	{
		$tempFile = $args[0]+".tmp";
		Write-Host "Getting content from: "$args[0];
		Write-Host "Writing to temp file: " $tempFile;
		$newGuid = [System.Guid]::NewGuid().Guid;
		$newGuid = "Guid(""" + $newGuid + """)";
		Write-Host "Replacing with: " $newGuid;
		
		Get-Content $args[0] |
			%{$_ -replace 'Guid\(([^\)]+)\)',$newGuid} > $tempFile

		move-item $tempFile $args[0] -force;
		
	}else{
		Write-Host "File Not Found";
	}
}else{
	Write-Host "Provide file path as argument";
}


  