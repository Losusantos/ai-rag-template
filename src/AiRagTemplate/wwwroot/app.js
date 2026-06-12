"use strict";

const messagesEl = document.getElementById("messages");
const formEl = document.getElementById("chat-form");
const inputEl = document.getElementById("input");
const sendEl = document.getElementById("send");

/**
 * メッセージ要素を生成して一覧へ追加する。
 * textContent を使い、モデル出力やエラーを HTML として解釈させない (XSS 対策)。
 */
function appendMessage(role, text, sources) {
  const wrapper = document.createElement("div");
  wrapper.className = `msg ${role}`;
  wrapper.textContent = text;

  if (Array.isArray(sources) && sources.length > 0) {
    const sourcesEl = document.createElement("div");
    sourcesEl.className = "sources";
    sourcesEl.appendChild(document.createTextNode("出典: "));
    for (const source of sources) {
      const tag = document.createElement("span");
      tag.textContent = source;
      sourcesEl.appendChild(tag);
    }
    wrapper.appendChild(sourcesEl);
  }

  messagesEl.appendChild(wrapper);
  messagesEl.scrollTop = messagesEl.scrollHeight;
  return wrapper;
}

async function send(message) {
  appendMessage("user", message);
  const pending = appendMessage("assistant", "考え中…");
  setBusy(true);

  try {
    const res = await fetch("/api/chat", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ message }),
    });

    const body = await res.json().catch(() => null);

    if (!res.ok || !body || !body.success) {
      const error = (body && body.error) || `エラーが発生しました (HTTP ${res.status})`;
      pending.className = "msg error";
      pending.textContent = error;
      return;
    }

    pending.textContent = body.data.answer || "(空の回答)";
    if (Array.isArray(body.data.sources) && body.data.sources.length > 0) {
      const sourcesEl = document.createElement("div");
      sourcesEl.className = "sources";
      sourcesEl.appendChild(document.createTextNode("出典: "));
      for (const source of body.data.sources) {
        const tag = document.createElement("span");
        tag.textContent = source;
        sourcesEl.appendChild(tag);
      }
      pending.appendChild(sourcesEl);
    }
  } catch (err) {
    pending.className = "msg error";
    pending.textContent = "通信に失敗しました。サーバーが起動しているか確認してください。";
  } finally {
    setBusy(false);
  }
}

function setBusy(busy) {
  sendEl.disabled = busy;
  inputEl.disabled = busy;
  if (!busy) {
    inputEl.focus();
  }
}

formEl.addEventListener("submit", (event) => {
  event.preventDefault();
  const message = inputEl.value.trim();
  if (message.length === 0) {
    return;
  }
  inputEl.value = "";
  void send(message);
});

inputEl.addEventListener("keydown", (event) => {
  if (event.key === "Enter" && !event.shiftKey) {
    event.preventDefault();
    formEl.requestSubmit();
  }
});

inputEl.focus();
