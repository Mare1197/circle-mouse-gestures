const { spawn } = require('node:child_process');
const path = require('node:path');

const rootDir = path.resolve(__dirname, '..');
const npmCommand = process.platform === 'win32' ? 'npm.cmd' : 'npm';

let watchProcess;
let hotProcess;

function runBuild() {
  return new Promise((resolve, reject) => {
    const buildProcess = spawn(npmCommand, ['run', 'build'], {
      cwd: rootDir,
      stdio: 'inherit'
    });

    buildProcess.on('exit', (code) => {
      if (code === 0) {
        resolve();
        return;
      }

      reject(new Error(`Build failed with exit code ${code}`));
    });
  });
}

function stopProcess(proc) {
  if (!proc || proc.killed) {
    return;
  }

  proc.kill('SIGTERM');
}

function shutdown(code = 0) {
  stopProcess(hotProcess);
  stopProcess(watchProcess);
  process.exit(code);
}

function startWatchAndDesktop() {
  watchProcess = spawn(npmCommand, ['run', 'watch'], {
    cwd: rootDir,
    stdio: 'inherit'
  });

  hotProcess = spawn(npmCommand, ['run', 'start:desktop:hot'], {
    cwd: rootDir,
    stdio: 'inherit'
  });

  watchProcess.on('exit', (code) => {
    if (code !== 0) {
      console.error(`[desktop] webpack watch exited with code ${code}`);
      shutdown(code ?? 1);
    }
  });

  hotProcess.on('exit', (code) => {
    shutdown(code ?? 0);
  });
}

process.on('SIGINT', () => shutdown(0));
process.on('SIGTERM', () => shutdown(0));

runBuild()
  .then(() => {
    startWatchAndDesktop();
  })
  .catch((error) => {
    console.error(`[desktop] ${error.message}`);
    process.exit(1);
  });
