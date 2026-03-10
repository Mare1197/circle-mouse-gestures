const { spawn } = require('node:child_process');
const { watch } = require('node:fs');
const path = require('node:path');

const rootDir = path.resolve(__dirname, '..');
const electronBin = require('electron');

let electronProcess;
let restartTimer;

function startElectron() {
  electronProcess = spawn(electronBin, ['.'], {
    cwd: rootDir,
    stdio: 'inherit'
  });

  electronProcess.on('exit', (code, signal) => {
    if (signal !== 'SIGTERM') {
      process.exit(code ?? 0);
    }
  });
}

function restartElectron() {
  if (!electronProcess) {
    startElectron();
    return;
  }

  electronProcess.once('exit', () => {
    startElectron();
  });

  electronProcess.kill('SIGTERM');
}

function queueRestart() {
  clearTimeout(restartTimer);
  restartTimer = setTimeout(() => {
    console.log('[desktop] Change detected. Restarting Electron...');
    restartElectron();
  }, 200);
}

for (const relativePath of ['dist', 'desktop']) {
  const targetPath = path.join(rootDir, relativePath);

  watch(targetPath, { recursive: true }, (eventType, filename) => {
    if (!filename) {
      return;
    }

    if (eventType === 'change' || eventType === 'rename') {
      queueRestart();
    }
  });
}

process.on('SIGINT', () => {
  if (electronProcess) {
    electronProcess.kill('SIGTERM');
  }

  process.exit(0);
});

startElectron();
