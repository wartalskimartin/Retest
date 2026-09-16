# Copilot Instructions

## Project Guidelines
- User's project uses DevExpress.Blazor version 23.2.3 and solutions should be compatible with this older version.

## Data Handling Instructions
- When auto-filling values down grid rows, do not fill records from previous shifts (IsFromPreviousShift == true).
- Outage detection must not rely on idle time gaps, considering that tablet users do not work continuously and office Wi-Fi is intentionally unavailable/weak.