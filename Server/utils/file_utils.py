import os

def remove_file(file_path: str):
    """Безпечне видалення файлу, якщо він існує."""
    if os.path.exists(file_path):
        os.remove(file_path)
