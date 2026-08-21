"""Add healthcheck for Sockets and "live" status bubble on Socket page (fix for issue #48)"""

import subprocess
from datetime import datetime
from pathlib import Path


class SocketHealthChecker:
    """Check health of Unix domain sockets and provide live status information."""

    def __init__(self, sockets: list[str]):
        self.sockets = sockets
        self.results = {}

    def _socket_exists(self, socket_path: str) -> bool:
        return Path(socket_path).exists()

    def _socket_writable(self, socket_path: str) -> bool:
        return Path(socket_path).is_socket() and Path(socket_path).stat().st_mode & 0o200

    def _check_port_response(self, socket_path: str) -> bool:
        try:
            result = subprocess.run(
                ["socat", "-u", f"UNIX-CONNECT:{socket_path}", "-"],
                capture_output=True,
                text=True,
                timeout=5
            )
            return result.returncode == 0
        except (subprocess.TimeoutExpired, FileNotFoundError):
            return False

    def check_all(self) -> dict[str, dict]:
        for socket in self.sockets:
            status = {
                "exists": self._socket_exists(socket),
                "writable": False,
                "responding": False,
                "last_checked": datetime.now().isoformat(),
                "status": "offline"
            }

            if status["exists"]:
                status["writable"] = self._socket_writable(socket)
                status["responding"] = self._check_port_response(socket)

                if status["responding"]:
                    status["status"] = "online"
                elif status["writable"]:
                    status["status"] = "degraded"
                else:
                    status["status"] = "offline"

            self.results[socket] = status

        return self.results


def get_healthcheck() -> dict:
    """Return socket health check results for use in Socket page."""
    # Example socket paths - replace with actual configuration paths
    socket_paths = [
        "/var/run/docker.sock",
        "/run/dbus/system_bus_socket"
    ]

    checker = SocketHealthChecker(socket_paths)
    return {
        "healthy": True,
        "timestamp": datetime.now().isoformat(),
        "sockets": checker.check_all()
    }


if __name__ == "__main__":
    import json
    print(json.dumps(get_healthcheck(), indent=2))

