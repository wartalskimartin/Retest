# Copilot Instructions

## Project Guidelines
- User's project uses DevExpress.Blazor version 23.2.3 and solutions should be compatible with this older version.

## Data Handling Instructions
- When auto-filling values down grid rows, do not fill records from previous shifts (IsFromPreviousShift == true). Rows with IsFromPreviousShift == true on the Retest page must be highlighted, must not be selectable or editable, and should automatically get prev_shift marker values during save.
- Outage detection must not rely on idle time gaps, considering that tablet users do not work continuously and office Wi-Fi is intentionally unavailable/weak.

## Grid Editing Instructions
- Use inline edit mode (GridEditMode.EditRow) for the DxGrid. Inline editing should be preserved as the default workflow, not PopupEditForm. The checkbox selection must work while row editors remain open.

## PWA Update Limitations
- In Blazor WebAssembly PWA, there is no way to update a VERY OLD version without user intervention (clearing cache). This is due to:
  1. Service Worker caching files using a cache-first strategy.
  2. HTTP Cache-Control headers being ignored (the Service Worker ignores the server).
  3. Client-side JavaScript (pwa-update.js, cache-clear.js) is also cached.
- For NEW installations, implement a network-first strategy in service-worker.published.js for navigate requests.
- For OLD versions, auto-update is not possible. The user MUST clear the cache or the app in settings. This is a limitation of PWA that cannot be resolved without client intervention.