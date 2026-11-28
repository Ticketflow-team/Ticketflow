#!/usr/bin/env bash
set -euo pipefail

OUT_DIR="tests/ai-redteam/out"
mkdir -p "$OUT_DIR/.promptfoo"
[ -n "${GEMINI_API_KEY:-}" ] || { echo "::error::GEMINI_API_KEY is not set."; exit 1; }

export REST_API_KEY="$GEMINI_API_KEY"
export PROMPTFOO_CONFIG_DIR="$PWD/$OUT_DIR/.promptfoo"
PROMPTFOO_STATUS="FAIL"
GARAK_STATUS="FAIL"

echo "=== [1/2] Running promptfoo (6 tests @ <=5 RPM) ==="
if npx --yes promptfoo@0.126.0 eval -c tests/ai-redteam/promptfooconfig.yaml --output "$OUT_DIR/promptfoo-results.json" -j 1 --delay 13000 --no-cache; then
  PROMPTFOO_STATUS="PASS"
fi

sleep 15
echo "=== [2/2] Running garak (3 probes @ <=5 RPM) ==="
if garak --model_type rest --model_name gemini-4-hydrogen --generator_option_file tests/ai-redteam/garak-rest.json --probes promptinject.HijackKillHumansMini --probe_options '{"promptinject":{"HijackKillHumansMini":{"max_prompts":3}}}' --generations 1 --parallel_attempts 1 --report_prefix garak-redteam; then
  GARAK_STATUS="PASS"
fi

GARAK_RUNS_DIR="${XDG_DATA_HOME:-$HOME/.local/share}/garak/garak_runs"
cp -f "$GARAK_RUNS_DIR"/garak-redteam* "$OUT_DIR/" 2>/dev/null || true

printf "# Rapport Red-Team LLM — TicketFlow (P13)\n\n- Modèle : gemini-4-hydrogen\n- Date UTC : %s\n- promptfoo : **%s**\n- garak : **%s**\n" "$(date -u +"%Y-%m-%dT%H:%M:%SZ")" "$PROMPTFOO_STATUS" "$GARAK_STATUS" > "$OUT_DIR/llm-redteam-report.md"
cat "$OUT_DIR/llm-redteam-report.md"

[ "$PROMPTFOO_STATUS" = "PASS" ] && [ "$GARAK_STATUS" = "PASS" ]
