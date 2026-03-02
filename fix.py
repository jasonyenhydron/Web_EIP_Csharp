import os, re

files = [
    r'D:\CODE\Web_EIP_Csharp\Views\Components\EipSearchBoxTagHelper.cs',
    r'D:\CODE\Web_EIP_Csharp\Views\Components\GSearchBoxTagHelper.cs',
    r'D:\CODE\Web_EIP_Csharp\Views\Components\GIframeModalTagHelper.cs',
    r'D:\CODE\Web_EIP_Csharp\Views\Components\GDataGridTagHelper.cs',
    r'D:\CODE\Web_EIP_Csharp\Views\Components\GLayoutTagHelper.cs',
    r'D:\CODE\Web_EIP_Csharp\Views\Components\GPanelTagHelper.cs',
    r'D:\CODE\Web_EIP_Csharp\Views\Components\GAlertTagHelper.cs',
    r'D:\CODE\Web_EIP_Csharp\Views\Components\GButtonTagHelper.cs',
    r'D:\CODE\Web_EIP_Csharp\Views\Components\GEmptyStateTagHelper.cs',
    r'D:\CODE\Web_EIP_Csharp\Views\Components\GPageTitleTagHelper.cs'
]

variables = [
    'compId', 'Name', 'Placeholder', 'ApiUrl', 'TargetId', 'displayJs', 'valueJs', 'labelJs', 'Id', 'Class', 'disAttr',
    'listId', 'Url', 'Pagination', 'method', 'queryParams', 'Columns', 'headerJs', 'Title', 'SubTitle', 'Icon', 'Buttons',
    'SidebarWidth', 'HeaderUrl', 'SidebarUrl', 'MainUrl', 'SidebarVisible', 'encodeURIComponent(q)', 'encodeURIComponent',
    'closeJs', 'maximizeJs', 'restoreJs', 'modalId', 'overlayId', 'cardId', 'headerId', 'contentId', 'titleId',
    'xmodel', 'rdoAttr', 'reqAttr', 'maxAttr', 'minAttr', 'maxlenAttr', 'extraCls', 'inputHtml', 'helpHtml', 'required',
    'Label', 'Type', 'Value', 'Rows', 'tableId', 'paginationId', 'searchBoxId', 'tbId', 'cls', 'text', 'iconSvg', 'submitJs', 'onclickJs', 'tgtAttr', 'colClass'
]

for file in files:
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()

    # We only care if they are using $\"\"\"
    if '$\"\"\"' in content or '$$\"\"\"' in content:
        # Switch to $$\"\"\"
        content = content.replace('($\"\"\"', '($$\"\"\"')
        content = content.replace('=$\"\"\"', '=$$\"\"\"')
        content = content.replace('=> $\"\"\"', '=> $$\"\"\"')

        # Fix multiline end literal format
        # C# 11 requires closing \"\"\" on its own line
        # e.g. })();</script>\"\"\");  => })();</script>\n\"\"\");
        content = re.sub(r'([^\n\s])\"\"\"\);', r'\1\n            \"\"\");', content)
        content = re.sub(r'([^\n\s])\"\"\",', r'\1\n            \"\"\",', content)

        # In Javascript, replace {{ and }} with { and }
        # BUT only if we just changed to $$'''!
        content = content.replace('{{', '{').replace('}}', '}')

        # Now change C# interpolation vars from {x} to {{x}}
        for v in variables:
            content = content.replace('{' + v + '}', '{{' + v + '}}')
            content = content.replace('{' + v + '?', '{{' + v + '?') # for things like {compId??""} maybe

        with open(file, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f'Fixed {file}')
