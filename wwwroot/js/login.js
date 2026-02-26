// 控制語言設定顯示/隱藏
document.getElementById('use_defaults').addEventListener('change', function(e) {
    const settings = document.getElementById('lang_settings');
    if (e.target.checked) {
        settings.classList.add('hidden');
    } else {
        settings.classList.remove('hidden');
    }
});

// 程式測試模式切換
document.getElementById('test_mode').addEventListener('change', function(e) {
    const form = document.getElementById('loginForm');
    const workDirInput = document.getElementById('work_dir');
    if (e.target.checked) {
        form.action = '/test/login';
        workDirInput.value = 'Test Mode (app_test)';
        console.log('Switched to Test Mode: /test/login');
    } else {
        form.action = '/login';
        workDirInput.value = '';
        console.log('Switched to Normal Mode: /login');
    }
});

// 登入表單提交處理
document.getElementById('loginForm').addEventListener('submit', function (e) {
    // 顯示處理中 Modal
    const loadingModal = document.getElementById('loadingModal');
    loadingModal.classList.remove('hidden');

    // 禁用提交按鈕
    const loginBtn = document.getElementById('loginBtn');
    loginBtn.disabled = true;
    loginBtn.innerHTML = '<svg class="w-5 h-5 animate-spin inline mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg>連線中...';

    // 更新載入訊息
    const messages = [
        '正在驗證連線參數...',
        '正在建立 TCP 連線...',
        '正在進行身份驗證...',
        '正在載入控制台...'
    ];

    let messageIndex = 0;
    const messageInterval = setInterval(function () {
        if (messageIndex < messages.length) {
            document.getElementById('loadingMessage').innerHTML =
                '<svg class="w-5 h-5 inline mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>' + messages[messageIndex];
            messageIndex++;
        }
    }, 1500);
});
