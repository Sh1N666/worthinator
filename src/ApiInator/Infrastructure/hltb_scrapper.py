import sys
import json
import asyncio
import logging

from howlongtobeatpy import HowLongToBeat

logging.basicConfig(stream=sys.stderr, level=logging.INFO, format='%(levelname)s: %(message)s')

async def _fetch_by_name(game_name: str) -> dict[str, object]:
    try:
        logging.info(f"Attempting to fetch game by name: {game_name}")
        results = await HowLongToBeat().async_search(game_name)

        if results is not None and len(results) > 0:
            best_match = results[0]
            logging.info(f"Successfully found game: {best_match.game_name}")
            return {
                "status": "success",
                "data": {
                    "game_id": best_match.game_id,
                    "game_name": best_match.game_name,
                    "main_story": best_match.main_story,
                    "main_extra": best_match.main_extra,
                    "completionist": best_match.completionist
                },
                "message": None
            }

        logging.warning(f"No results found for game name: {game_name}")
        return {"status": "error", "data": None, "message": "Game not found."}

    except asyncio.TimeoutError:
        logging.error(f"Timeout error occurred while searching for name: {game_name}")
        return {"status": "error", "data": None, "message": "Request timed out."}
    except Exception as e:
        logging.error(f"Unexpected error while searching by name '{game_name}': {str(e)}")
        return {"status": "error", "data": None, "message": f"An unexpected error occurred: {str(e)}"}


async def _fetch_by_id(game_id: int) -> dict[str, object]:
    try:
        logging.info(f"Attempting to fetch game by ID: {game_id}")
        result = await HowLongToBeat().async_search_from_id(game_id)

        if result is not None:
            logging.info(f"Successfully found game ID: {game_id}")
            return {
                "status": "success",
                "data": {
                    "game_id": result.game_id,
                    "game_name": result.game_name,
                    "main_story": result.main_story,
                    "main_extra": result.main_extra,
                    "completionist": result.completionist
                },
                "message": None
            }

        logging.warning(f"No results found for game ID: {game_id}")
        return {"status": "error", "data": None, "message": "Game ID not found."}

    except asyncio.TimeoutError:
        logging.error(f"Timeout error occurred while searching for ID: {game_id}")
        return {"status": "error", "data": None, "message": "Request timed out."}
    except Exception as e:
        logging.error(f"Unexpected error while searching by ID '{game_id}': {str(e)}")
        return {"status": "error", "data": None, "message": f"An unexpected error occurred: {str(e)}"}


def fetch_game_by_name(search_value: str) -> dict[str, object]:
    return asyncio.run(_fetch_by_name(search_value))


def fetch_game_by_id(search_value: int) -> dict[str, object]:
    return asyncio.run(_fetch_by_id(search_value))