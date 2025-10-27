import { useState, useEffect, useRef } from "react";
import { Send } from "lucide-react";
import api from "../services/api.service";
import type { ChatMessage, User } from "../services/api.service";

interface ChatViewProps {
  teamId: number;
  userName: string;
}

export function ChatView({ teamId, userName }: ChatViewProps) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [newMessage, setNewMessage] = useState<string>("");
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    fetchMessages();
    const interval = setInterval(fetchMessages, 2000);
    return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teamId]);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  const fetchMessages = async () => {
    try {
      const data = await api.chatsApi.getAll(teamId);
      setMessages(data);
      // also refresh users so we can display role badges
      try {
        const u = await api.usersApi.getAll(teamId);
        setUsers(u);
      } catch (e) {
        console.warn("Failed to fetch users for roles", e);
      }
    } catch (error) {
      console.error("Error fetching messages:", error);
    }
  };

  const sendMessage = async () => {
    if (!newMessage.trim()) return;

    try {
      await api.chatsApi.add(teamId, { text: newMessage, userName: userName });
      setNewMessage("");
      fetchMessages();
    } catch (error) {
      console.error("Error sending message:", error);
    }
  };

  return (
    <div className="bg-slate-900 rounded-lg p-6 h-[600px] flex flex-col">
      <h2 className="text-xl font-semibold text-white mb-4">Chat</h2>

      <div className="flex-1 overflow-y-auto space-y-3 mb-4">
        {messages.map((msg) => {
          const sender = users.find((u) => u.name === msg.userName);
          const role = sender?.role ?? null;
          return (
            <div key={msg.id} className="bg-slate-800 p-3 rounded-lg">
              <div className="flex items-center gap-2">
                <p className="text-yellow-400 text-sm font-semibold">
                  {msg.userName}
                </p>
                {role && (
                  <span
                    className={`text-xs px-2 py-0.5 rounded ${
                      role === "Owner"
                        ? "bg-yellow-500 text-black"
                        : role === "Moderator"
                        ? "bg-indigo-600 text-white"
                        : "bg-slate-700 text-slate-200"
                    }`}
                  >
                    {role}
                  </span>
                )}
              </div>
              <p className="text-white">{msg.text}</p>
              <p className="text-slate-500 text-xs mt-1">
                {new Date(msg.timestamp).toLocaleTimeString()}
              </p>
            </div>
          );
        })}
        <div ref={messagesEndRef} />
      </div>

      <div className="flex gap-2">
        <input
          type="text"
          placeholder="Type a message..."
          value={newMessage}
          onChange={(e) => setNewMessage(e.target.value)}
          onKeyPress={(e) => e.key === "Enter" && sendMessage()}
          className="flex-1 px-4 py-2 bg-slate-800 text-white rounded-lg focus:outline-none focus:ring-2 focus:ring-yellow-500"
        />
        <button
          onClick={sendMessage}
          className="px-6 py-2 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition font-semibold"
        >
          <Send size={20} />
        </button>
      </div>
    </div>
  );
}
